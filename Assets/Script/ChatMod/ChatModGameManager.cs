using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class ChatModGameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gamePanel;
    public Transform spawnArea; 
    public GameObject commentPrefab; 
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    [Header("Configuration")]
    public float gameDuration = 15f;
    public float baseSpawnInterval = 0.8f;
    public float commentLifeTime = 2.0f;

    private float timer;
    private int score;
    private bool isPlaying = false;
    private float currentSpawnInterval;

    public void StartModGame()
    {
        if(gamePanel != null) 
        {
            gamePanel.SetActive(true);
            gamePanel.transform.SetAsLastSibling(); // Force Draw on TOP
            
            // NOTE: Removed auto-layout / black background logic to allow manual design.
        }
        
        // Ensure SpawnArea is accessible but respect manual position
        if(spawnArea != null)
        {
             // Just ensuring it's not null, no forced movement
        }
        // Brace removed correctly
        
        isPlaying = true;
        timer = gameDuration;
        score = 0;
        
        // Calculate Difficulty based on Morality
        float morality = 100f; 
        if(GameManager.Instance != null) morality = GameManager.Instance.morality;

        // Scaling:
        // High Morality (100) -> Slower spawn (Relaxed stream) -> 0.8s
        // Low Morality (0) -> Chaos spawn (Hate Raid) -> 0.35s
        currentSpawnInterval = Mathf.Lerp(0.35f, baseSpawnInterval, morality / 100f);

        UpdateUI();
        StartCoroutine(GameLoop(morality));
    }

    IEnumerator GameLoop(float morality)
    {
        while (timer > 0 && isPlaying)
        {
            SpawnComment(morality);
            yield return new WaitForSeconds(currentSpawnInterval);
            timer -= currentSpawnInterval; 
            UpdateUI();
        }
        EndGame();
    }
    
    void Update()
    {
        if(isPlaying)
        {
             timer -= Time.deltaTime;
             if(timerText) timerText.text = Mathf.CeilToInt(timer).ToString();
             if(timer <= 0) EndGame();
        }
    }

    private List<RectTransform> activeBubbles = new List<RectTransform>();

    void SpawnComment(float morality)
    {
        if(spawnArea == null || commentPrefab == null) return;

        // Clean up nulls
        activeBubbles.RemoveAll(x => x == null);

        // NOTE: Removed forced centering so user can place SpawnArea wherever they want.

        RectTransform areaRect = spawnArea.GetComponent<RectTransform>();
        
        float w = 100f;
        float h = 100f;

        if (areaRect != null)
        {
            w = Mathf.Max(10f, areaRect.rect.width / 2 - 60);
            h = Mathf.Max(10f, areaRect.rect.height / 2 - 40);
        }

        // Try to find a non-overlapping position
        Vector2 finalPos = Vector2.zero;
        bool foundPos = false;
        
        for(int i=0; i<10; i++)
        {
            Vector2 randomPos = new Vector2(Random.Range(-w, w), Random.Range(-h, h));
            bool overlap = false;
            foreach(var b in activeBubbles)
            {
                if(b != null && Vector2.Distance(b.anchoredPosition, randomPos) < 150f) // 150f is approx width of bubble
                {
                    overlap = true;
                    break;
                }
            }
            
            if(!overlap)
            {
                finalPos = randomPos;
                foundPos = true;
                break;
            }
        }
        
        if(!foundPos) return; // Skip spawn if too crowded (prevents overlap)

        GameObject obj = Instantiate(commentPrefab, spawnArea); 
        RectTransform rt = obj.GetComponent<RectTransform>();
        
        // Add to active list
        activeBubbles.Add(rt);
        
        // Force Anchors/Pivot to Center so (0,0,0) is actually the middle
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        
        rt.anchoredPosition3D = new Vector3(finalPos.x, finalPos.y, 0f); 
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;

        // Debugging
        Canvas debugCanvas = rt.GetComponentInParent<Canvas>();
        if(debugCanvas == null) Debug.LogError("ChatMod: Spawned Object is NOT under a Canvas! It will be invisible.");
        
        // Debug.Log($"ChatMod: Spawned at LocalPos: {rt.localPosition}, W: {w}, H: {h}"); 
        // Debug.Log($"Environment: TimeScale={Time.timeScale}, SpawnAreaScale={spawnArea.localScale}"); 

        // Determine Type based on Morality
        // High Morality -> Mostly Good Comments (Don't Click)
        // Low Morality -> Mostly Bad Comments (Click)
        
        // Chance for Hate:
        // Morality 100 -> 20% Hate (Occasional Troll)
        // Morality 0 -> 90% Hate (Full Raid)
        float hateChance = Mathf.Lerp(0.9f, 0.2f, morality / 100f);
        
        bool isHate = Random.value < hateChance;
        string text = "";
        
        // Retrieve from ChatManager Shared Database
        if (isHate)
        {
            // Bad or Bot
            string[] bad = ChatManager.badMessages;
            string[] bot = ChatManager.botMessages;
            
            // Mix sources
            if (bad != null && bot != null && bad.Length > 0)
            {
                 // 70% real hate, 30% bot
                 text = (Random.value < 0.7f || bot.Length == 0) 
                        ? bad[Random.Range(0, bad.Length)] 
                        : bot[Random.Range(0, bot.Length)];
            }
            else
            {
                text = "HATE!"; // Fallback
            }
        }
        else
        {
            // Good
            string[] good = ChatManager.goodMessages;
            if (good != null && good.Length > 0)
                text = good[Random.Range(0, good.Length)];
            else
                text = "LOVE!"; 
        }
        
        // Colors
        Color c = isHate ? new Color(1f, 0.4f, 0.4f) : new Color(0.4f, 1f, 0.6f);

        // Init Script
        ChatBubbleItem item = obj.GetComponent<ChatBubbleItem>();
        if(item != null) item.Setup(text, isHate, c, commentLifeTime, this);
        else Destroy(obj, commentLifeTime);
    }

    public void OnCommentClicked(ChatBubbleItem item)
    {
        if(!isPlaying) return;

        if (item.isHate)
        {
            // Correct Action: Banned a hater
            score += 100;
        }
        else
        {
            // Wrong Action: Banned a fan
            score -= 50; 
        }
        UpdateUI();
        Destroy(item.gameObject);
    }

    public void OnCommentExpired(ChatBubbleItem item)
    {
        if(!isPlaying) return;
        
        if(item.isHate)
        {
            // Missed a hater -> They pollute chat
            score -= 20; 
        }
        else
        {
            // Fan message stayed -> Good vibes
            score += 20; 
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        if(scoreText) scoreText.text = "Puan: " + score;
    }

    void EndGame()
    {
        if(!isPlaying) return;
        isPlaying = false;
        StopAllCoroutines();

        if(MainController.Instance != null)
        {
            MainController.Instance.OnChatModFinished(Mathf.Max(0, score));
        }

        if(gamePanel != null) gamePanel.SetActive(false);
        
        if(spawnArea != null)
        {
             foreach(Transform t in spawnArea) Destroy(t.gameObject);
        }
    }
}

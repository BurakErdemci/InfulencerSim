using UnityEngine;
using System.Collections;
using TMPro; 
using UnityEngine.UI; 

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;
    void Awake() { Instance = this; }

    [Header("Prefab Ayarları")]
    public GameObject playerPrefab;
    public GameObject[] goodItems; 
    public GameObject[] badItems;  
    
    [Header("UI Elementleri")]
    public Button startGameButton;    
    public TextMeshProUGUI scoreText; 
    public TextMeshProUGUI timerText; 
    
    [Header("Görsellik")]
    public GameObject gameBackground;
    
    // --- YENİ AYAR: Buradan Z'yi elle değiştirebilirsin ---
    public float objectZPos = -5f; 
    // -----------------------------------------------------

    [Header("Oyun Ayarları")]
    public float gameDuration = 20f;  
    public float spawnY = 6f; 
    public float spawnXRange = 3f;

    private bool isPlaying = false;
    private GameObject currentPlayer;
    private int currentScore = 0;

    public void SetupMinigame()
    {
        Cleanup();
        currentScore = 0;
        if(scoreText != null) scoreText.text = "Skor: 0";
        if(timerText != null) timerText.text = gameDuration.ToString();

        if(startGameButton != null) startGameButton.gameObject.SetActive(true);
        if(gameBackground != null) gameBackground.SetActive(false); 

        if(startGameButton != null) 
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(StartMinigameLogic);
        }
    }

    void StartMinigameLogic()
    {
        if(startGameButton != null) startGameButton.gameObject.SetActive(false);
        if(gameBackground != null) gameBackground.SetActive(true);

        isPlaying = true;
        
        if(currentPlayer != null) Destroy(currentPlayer);
        currentPlayer = Instantiate(playerPrefab, transform);
        
        // YENİ: Z değerini yukarıdaki değişkenden alıyor
        currentPlayer.transform.localPosition = new Vector3(0, -3.5f, objectZPos); 
        currentPlayer.tag = "Player"; 

        StartCoroutine(SpawnRoutine());
        StartCoroutine(GameTimerRoutine());
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        if(scoreText != null) scoreText.text = "Skor: " + currentScore;
    }

    IEnumerator SpawnRoutine()
    {
        while (isPlaying)
        {
            SpawnRandomItem();
            yield return new WaitForSeconds(0.5f); 
        }
    }
    
    void SpawnRandomItem()
    {
         GameObject[] pool = Random.value > 0.4f ? goodItems : badItems;
         if (pool.Length == 0) return;

         GameObject prefab = pool[Random.Range(0, pool.Length)];
         float randomX = Random.Range(-spawnXRange, spawnXRange);
         
         // YENİ: Z değerini yukarıdaki değişkenden alıyor
         Vector3 spawnPos = new Vector3(randomX, spawnY, objectZPos); 
         
         Instantiate(prefab, transform).transform.localPosition = spawnPos;
    }

    IEnumerator GameTimerRoutine()
    {
        float timeLeft = gameDuration;
        while (timeLeft > 0 && isPlaying)
        {
            if(timerText != null) timerText.text = Mathf.Ceil(timeLeft).ToString();
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
        EndMinigame();
    }

    public void EndMinigame()
    {
        isPlaying = false;
        Cleanup();
        if(MainController.Instance != null) MainController.Instance.CompleteStreamSession(currentScore);
    }

    void Cleanup()
    {
        if(currentPlayer != null) Destroy(currentPlayer);
        FallingObject[] allItems = FindObjectsByType<FallingObject>(FindObjectsSortMode.None);
        foreach(var item in allItems) Destroy(item.gameObject);
    }
}
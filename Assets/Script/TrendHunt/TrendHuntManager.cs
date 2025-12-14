using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class TrendHuntManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject trendHuntPanel;
    public Transform container; 
    public GameObject trendItemPrefab;
    public TextMeshProUGUI timerText;
    public Button finishButton;

    [Header("Configuration")]
    public float phaseDuration = 10f;
    public int maxSelectable = 3;

    private List<TrendItem> selectedTrends = new List<TrendItem>();
    private float timer;
    private bool isActive = false;

    // --- BAŞLIKLAR ---
    private string[] safeTopics = { 
        "ASMR Slicing", "Setup Tour", "Get Ready With Me", "Unboxing Ps5", "Tech Review", 
        "Speedrun Any%", "Cooking Fails", "Reacting to TikToks", "Honest Q&A", "Storytime", 
        "Daily Vlog", "Fitness Challenge", "Room Tour 2025", "Makeup Tutorial", "DIY Hacks", 
        "Morning Routine", "Travel Japan", "Gaming Highlights", "Cute Cat Video", "Life Hacks"
    };

    private string[] riskyTopics = { 
        "EXPOSED!", "I'm Quitting...", "Huge Drama", "Leaked DMs", "Fake Tears", 
        "Controversial Take", "Clickbait Master", "Faked Prank", "Stolen Content", "View Botting", 
        "Scam Alert", "Rage Quit", "Ban Appeal", "Diss Track", "Relationship Drama"
    };

    public void StartTrendHunt()
    {
        if(trendHuntPanel != null) trendHuntPanel.SetActive(true);
        isActive = true;
        timer = phaseDuration;
        selectedTrends.Clear();

        if (finishButton != null)
        {
            finishButton.onClick.RemoveAllListeners();
            finishButton.onClick.AddListener(ManualFinish);
        }

        GenerateTrends();
        StartCoroutine(TimerRoutine());
    }

    void GenerateTrends()
    {
        if (container == null) return;
        foreach (Transform child in container) Destroy(child.gameObject);

        int riskyCount = Random.Range(2, 4);
        for (int i = 0; i < riskyCount; i++) SpawnItem(GetRandomTopic(riskyTopics), true);

        int safeCount = Random.Range(4, 6);
        for (int i = 0; i < safeCount; i++) SpawnItem(GetRandomTopic(safeTopics), false);
    }

    string GetRandomTopic(string[] source)
    {
        return source[Random.Range(0, source.Length)];
    }

    void SpawnItem(string topic, bool isRisky)
    {
        if(trendItemPrefab == null) return;

        GameObject go = Instantiate(trendItemPrefab, container);
        TrendItem item = go.GetComponent<TrendItem>();
        
        int gain = 0;
        float moralityChange = 0; // Değişken adını düzelttim: moralityChange

        if (isRisky)
        {
            gain = Random.Range(500, 1000);
            
            
            // Riskli ise NEGATİF değer veriyoruz ki düşsün.
            
            if (GameManager.Instance.streamCount == 0)
            {
                // İlk yayın için hafif ceza (-5)
                moralityChange = -5f; 
            }
            else
            {
                // Sonraki yayınlar için ağır ceza (-15 ile -25 arası)
                moralityChange = Random.Range(-25f, -15f); 
            }
        }
        else
        {
            gain = Random.Range(50, 200);
            // Güvenli içerik akıl sağlığını biraz iyileştirir (+2)
            moralityChange = 2f; 
        }

        // moralityChange değerini item'a gönderiyoruz
        item.Setup(topic, gain, moralityChange, isRisky, this);
    }

    public bool TrySelectTrend(TrendItem item)
    {
        if (selectedTrends.Count >= maxSelectable) return false;
        selectedTrends.Add(item);
        return true;
    }

    public void DeselectTrend(TrendItem item)
    {
        selectedTrends.Remove(item);
    }

    public void ManualFinish()
    {
        if (!isActive) return;
        FinishPhase();
    }

    IEnumerator TimerRoutine()
    {
        while (timer > 0 && isActive)
        {
            if(timerText != null) timerText.text = Mathf.Ceil(timer).ToString();
            yield return null;
            timer -= Time.deltaTime;
        }
        FinishPhase();
    }

    void FinishPhase()
    {
        isActive = false;
        StopAllCoroutines();

        int totalGain = 0;
        float totalMoralityChange = 0;

        foreach (var t in selectedTrends)
        {
            totalGain += t.followerGain;
            totalMoralityChange += t.moralityLoss; // TrendItem scriptinde bu değişkenin adı moralityLoss ise dokunma, değeri negatif zaten
        }

        // GameManager'a gönder (Negatif olduğu için düşürecek)
        if(GameManager.Instance != null)
            GameManager.Instance.UpdateGeneralStats(totalGain, totalMoralityChange);

        if(trendHuntPanel != null) trendHuntPanel.SetActive(false);
        if(MainController.Instance != null) MainController.Instance.OnTrendHuntFinished();
    }
}
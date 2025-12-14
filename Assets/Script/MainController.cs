using UnityEngine;
using UnityEngine.UI; 
using TMPro;          
using System.Collections;
using DG.Tweening; 

public class MainController : MonoBehaviour
{
    public static MainController Instance;

    [Header("--- YÖNETİCİLER ---")]
    [SerializeField] private StreamUIManager streamUIManager; 
    [SerializeField] private DialogueManager dialogueManager;         
    [SerializeField] private TrendHuntManager trendHuntManager; // [NEW]
    [SerializeField] private ChatModGameManager chatModManager; // [NEW_GAME]

    [Header("--- ANA EKRAN UI ---")]
    [SerializeField] private TextMeshProUGUI mainFollowerText; 
    [SerializeField] private Button startStreamButton;         
    
    [Header("--- YAYIN EKRANI UI ---")]
    [SerializeField] private TextMeshProUGUI liveViewerText; 

    [Header("--- SONUÇ PANELI ---")]
    [SerializeField] private GameObject resultPanel;           
    [SerializeField] private TextMeshProUGUI resultGainText;     
    [SerializeField] private TextMeshProUGUI resultSanityText;   
    [SerializeField] private Button continueButton;            
    
    [Header("--- MINIGAME AYARLARI ---")]
    // Çift değişkeni sildim, sadece bunları kullanacağız:
    [SerializeField] private MinigameManager minigameScript; 
    [SerializeField] private GameObject minigameObject;      

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateMainUI();
        if(resultPanel != null) resultPanel.SetActive(false);
        
        if (startStreamButton != null)
        {
            startStreamButton.onClick.RemoveAllListeners();
            startStreamButton.onClick.AddListener(StartButtonLogic);
        }

        if(continueButton != null) 
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(EndStreamSession);
        }

        StartCoroutine(AutoSpeakAtStart());
    }

    IEnumerator AutoSpeakAtStart()
    {
        yield return new WaitForSeconds(1.0f);
        if(dialogueManager != null) dialogueManager.SpeakInRoom(); 
    }

    // [MODIFIED] Button now triggers Trend Hunt first
    public void StartButtonLogic()
    {
        startStreamButton.interactable = false;
        
        // Eğer TrendManager bağlıysa önce onu çalıştır
        if (trendHuntManager != null)
        {
            trendHuntManager.StartTrendHunt();
        }
        else
        {
            // Bağlı değilse direkt eski akış
            OnTrendHuntFinished();
        }
    }
    
    // [NEW] Trend Hunt bitince burası çağrılır
    public void OnTrendHuntFinished()
    {
        // Update UI in case we gained followers from trends
        UpdateMainUI(); 
        
        if (chatModManager != null)
        {
            chatModManager.StartModGame();
        }
        else
        {
            StartCoroutine(IntroSequence());
        }
    }

    public void OnChatModFinished(int score)
    {
        // Score effects? Maybe add to main followers?
        // Let's add score * 0.5 as followers, or just pure followers
        if(GameManager.Instance != null && score > 0)
        {
            GameManager.Instance.UpdateStats(score, 0); // Bonus score
        }

        UpdateMainUI();
        StartCoroutine(IntroSequence());
    }

    // Eski StartFullStreamSession -> IntroSequence olarak devam ediyor
    IEnumerator IntroSequence()
    {
        // 1. Balon varsa kapat
        if(dialogueManager != null) dialogueManager.HideBubbleImmediately();
        
        // 2. Canlı sayısını ayarla
        CalculateLiveViewers();

        // 3. Sahne Geçişi (Oda -> Yayın)
        if(streamUIManager != null) streamUIManager.GoLive();
        
        yield return new WaitForSeconds(0.5f); 

        // 4. Karakter "Selam" desin
        if(dialogueManager != null) dialogueManager.SpeakInChat();

        // 5. MINIGAME HAZIRLIĞI
        if(minigameObject != null) 
        {
            minigameObject.SetActive(true); // Paneli aç
            
            // Minigame'i "Butonu Göster" moduna getir
            if(minigameScript != null) minigameScript.SetupMinigame();
        }
        
        // NOT: Artık süre sayacı yok. Minigame bitince bizi çağıracak.
    }

    // --- BU FONKSİYONU MINIGAME BİTİNCE ÇAĞIRACAK ---
    public void CompleteStreamSession(int score)
    {
        // 1. Minigame'i gizle
        if(minigameObject != null) minigameObject.SetActive(false);

        // 2. İstatistikleri işle (Skor = Takipçi, -10 Akıl)
        float moralityLoss = 10f; 
        GameManager.Instance.UpdateStats(score, moralityLoss);

        // 3. Sonuç Panelini Aç
        if(liveViewerText != null) liveViewerText.transform.DOKill(); 
        OpenResultPanel(score, moralityLoss);
    }

    void OpenResultPanel(int gain, float sanityLoss)
    {
        if(resultPanel != null)
        {
            resultPanel.SetActive(true);
            resultPanel.transform.localScale = Vector3.zero;
            resultPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            
            if(resultGainText != null) resultGainText.text = "+" + gain.ToString() + " Takipçi";
            if(resultSanityText != null) resultSanityText.text = "-" + sanityLoss.ToString() + " Akıl";
        }
    }

    // --- DEVAM ET BUTONUNA BASINCA ÇALIŞIR ---
    public void EndStreamSession()
    {
        // DÜZELTME: Buradan Minigame'i tekrar bitirmeye çalışma (Döngüye girer).
        // Sadece paneli kapat ve odaya dön.
        
        if(minigameObject != null) minigameObject.SetActive(false); // Garanti olsun diye gizle

        if(resultPanel != null) resultPanel.SetActive(false);
        if(streamUIManager != null) streamUIManager.EndStream();
        
        UpdateMainUI();
        startStreamButton.interactable = true;
    }

    void UpdateMainUI()
    {
        if (mainFollowerText != null)
            mainFollowerText.text = "Takipçi: " + GameManager.Instance.followers.ToString();
    }

    void CalculateLiveViewers()
    {
        if(liveViewerText != null)
        {
            long totalFollowers = GameManager.Instance.followers;
            long liveCount = totalFollowers / 4;
            if (liveCount < 10) liveCount = 10; 

            liveViewerText.text = "🔴 " + liveCount.ToString();
            
            liveViewerText.transform.DOKill();
            liveViewerText.transform.localScale = Vector3.one;
            liveViewerText.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
    }
}

using UnityEngine;
using UnityEngine.UI; 
using TMPro;          
using System.Collections;
using DG.Tweening; 
using UnityEngine.SceneManagement; 

public class MainController : MonoBehaviour
{
    public static MainController Instance;

    [Header("--- YÖNETİCİLER ---")]
    [SerializeField] private StreamUIManager streamUIManager; 
    [SerializeField] private DialogueManager dialogueManager;         
    [SerializeField] private TrendHuntManager trendHuntManager;

    [Header("--- ANA EKRAN UI ---")]
    [SerializeField] private TextMeshProUGUI mainFollowerText; 
    [SerializeField] private Button startStreamButton;         
    [SerializeField] private Image corruptionOverlay; 
    
    [Header("--- YAYIN EKRANI UI ---")]
    [SerializeField] private TextMeshProUGUI liveViewerText; 

    [Header("--- SONUÇ PANELI ---")]
    [SerializeField] private GameObject resultPanel;           
    [SerializeField] private TextMeshProUGUI resultGainText;     
    [SerializeField] private TextMeshProUGUI resultSanityText;   
    [SerializeField] private Button continueButton;          

    [Header("--- THE OFFER (TEKLİF) PANELI ---")]
    [SerializeField] private GameObject offerPanel;        
    [SerializeField] private Button acceptOfferButton;     
    [SerializeField] private Button declineOfferButton;    

    [Header("--- MINIGAME AYARLARI ---")]
    [SerializeField] private MinigameManager minigameScript; 
    [SerializeField] private GameObject minigameObject;      

    [Header("--- ROOM UI ---")]
    [SerializeField] private TextMeshProUGUI roomFollowerText; 
    [SerializeField] private TextMeshProUGUI roomSanityText;   

    [Header("--- GOD MODE PANELİ ---")]
    [SerializeField] private GameObject godModePanel;
    [SerializeField] private Button acceptGodModeButton;
    [SerializeField] private Button declineGodModeButton;

    [Header("--- GAME OVER ---")]
    [SerializeField] private GameObject gameOverPanel; 
    [SerializeField] private Button returnToMenuButton; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateMainUI();
        if(resultPanel != null) resultPanel.SetActive(false);
        if(offerPanel != null) offerPanel.SetActive(false);
        if(godModePanel != null) godModePanel.SetActive(false);
        if(gameOverPanel != null) gameOverPanel.SetActive(false); 
        
        // 1. BUTONU GARANTİ KİLİTLE
        if (startStreamButton != null)
        {
            startStreamButton.interactable = false; // Önce kilitle
            startStreamButton.onClick.RemoveAllListeners();
            startStreamButton.onClick.AddListener(StartButtonLogic);
        }

        if(continueButton != null) 
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnNextButtonPressed); 
        }

        // Diğer buton bağlantıları...
        if (acceptOfferButton != null) acceptOfferButton.onClick.AddListener(() => OnOfferChoiceMade(true));
        if (declineOfferButton != null) declineOfferButton.onClick.AddListener(() => OnOfferChoiceMade(false));
        if (acceptGodModeButton != null) acceptGodModeButton.onClick.AddListener(() => OnGodModeChoice(true));
        if (declineGodModeButton != null) declineGodModeButton.onClick.AddListener(() => OnGodModeChoice(false));
        if (returnToMenuButton != null) returnToMenuButton.onClick.AddListener(ReturnToMainMenu);

        // Konuşmayı başlat
        StartCoroutine(AutoSpeakAtStart());
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            if (roomFollowerText != null) 
                roomFollowerText.text = GameManager.Instance.followers.ToString();

            if (roomSanityText != null) 
                roomSanityText.text = "%" + Mathf.RoundToInt(GameManager.Instance.morality).ToString(); 
            
            if (corruptionOverlay != null)
            {
                float ratio = 1.0f - (GameManager.Instance.morality / 100f);
                corruptionOverlay.color = new Color(0, 0, 0, ratio * 0.6f);
            }
        }
    }

    public void TriggerGameOver()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (minigameObject != null) minigameObject.SetActive(false);
        if (streamUIManager != null) streamUIManager.EndStream(); 

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.localScale = Vector3.zero;
            gameOverPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            if(startStreamButton) startStreamButton.interactable = false;
        }
    }

    public void ReturnToMainMenu()
    {
        GameManager.Instance.ResetGameData();
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator AutoSpeakAtStart()
    {
        // Biraz bekle ki sahne otursun
        yield return new WaitForSeconds(0.5f);
        
        if(dialogueManager != null) 
        {
            // Konuşma bitince butonu aç
            dialogueManager.SpeakInRoom(() => 
            {
                if (startStreamButton != null) startStreamButton.interactable = true;
            }); 
        }
        else
        {
            // Diyalog yoksa direkt aç
            if (startStreamButton != null) startStreamButton.interactable = true;
        }
    }

    public void StartButtonLogic()
    {
        if(startStreamButton) startStreamButton.interactable = false;
        
        if (trendHuntManager != null) trendHuntManager.StartTrendHunt();
        else OnTrendHuntFinished();
    }
    
    public void OnTrendHuntFinished()
    {
        UpdateMainUI(); 
        StartCoroutine(IntroSequence());
    }

    // --- DÜZELTİLEN KISIM: AKIŞ SIRASI ---
    IEnumerator IntroSequence()
    {
        // 1. Önceki panelleri temizle
        if(dialogueManager != null) dialogueManager.HideBubbleImmediately();
        CalculateLiveViewers();

        // 2. Sahne Geçişi
        if(streamUIManager != null) streamUIManager.GoLive();
        
        yield return new WaitForSeconds(0.8f); // Animasyon bitsin diye bekle

        // 3. ÖNCE Minigame'i getir ama BUTONU KİLİTLE
        if(minigameObject != null) 
        {
            minigameObject.SetActive(true); 
            if(minigameScript != null) 
            {
                minigameScript.SetupMinigame();
                minigameScript.SetStartButtonInteractable(false); // KİLİTLE!
            }
        }

        // 4. SONRA Konuşma Başlasın
        if(dialogueManager != null) 
        {
            // Konuşma BİTİNCE Minigame Butonunu AÇ
            dialogueManager.SpeakInChat(() => 
            {
                if(minigameScript != null) minigameScript.SetStartButtonInteractable(true);
            });
        }
        else
        {
            // Diyalog yoksa direkt aç
            if(minigameScript != null) minigameScript.SetStartButtonInteractable(true);
        }
    }

    public void CompleteStreamSession(int score)
    {
        if(minigameObject != null) minigameObject.SetActive(false);
        GameManager.Instance.ProcessMinigameEnd(score);
        
        if (GameManager.Instance.morality > 0)
        {
            ShowResults(score); 
        }
    }

    void ShowResults(int rawScore)
    {
        if(liveViewerText != null) liveViewerText.transform.DOKill(); 
        
        if(resultPanel != null)
        {
            resultPanel.SetActive(true);
            resultPanel.transform.localScale = Vector3.zero;
            resultPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            
            if(resultGainText != null) resultGainText.text = "Toplam Takipçi:\n" + GameManager.Instance.followers.ToString();
            
            if(resultSanityText != null) 
            {
                resultSanityText.text = "Akıl Sağlığı: %" + Mathf.RoundToInt(GameManager.Instance.morality).ToString();
                resultSanityText.color = (GameManager.Instance.morality > 50) ? Color.green : Color.red;
            }
        }
    }

    public void OnNextButtonPressed()
    {
        if(resultPanel != null) resultPanel.SetActive(false);
        ReturnToRoom();
        StartCoroutine(CheckOfferAfterRoomTransition());
    }

    IEnumerator CheckOfferAfterRoomTransition()
    {
        yield return new WaitForSeconds(1.5f); 

        if (GameManager.Instance.morality <= 0) yield break;

        if (GameManager.Instance.followers >= GameManager.Instance.nextEventThreshold)
        {
            if (GameManager.Instance.isCorrupt && !GameManager.Instance.isGodMode)
            {
                OpenGodModePanel();
            }
            else if (!GameManager.Instance.isCorrupt)
            {
                OpenOfferPanel();
            }
            else
            {
                // Teklif yoksa normal akış: Konuş ve Butonu kilitle
                StartCoroutine(AutoSpeakAtStart());
            }
        }
        else
        {
            // Normal akış
            StartCoroutine(AutoSpeakAtStart());
        }
    }

    void OpenGodModePanel()
    {
        if(godModePanel != null)
        {
            godModePanel.SetActive(true);
            godModePanel.transform.localScale = Vector3.zero;
            godModePanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            if(startStreamButton) startStreamButton.interactable = false;
        }
    }

    void OnGodModeChoice(bool accepted)
    {
        if(godModePanel != null) godModePanel.SetActive(false);

        if (accepted) GameManager.Instance.AcceptOffer(true); 
        else GameManager.Instance.PostponeOffer(true); 

        if (GameManager.Instance.morality > 0)
        {
            UpdateMainUI();
            StartCoroutine(AutoSpeakAtStart()); // Seçimden sonra konuşma
        }
    }

    void OpenOfferPanel()
    {
        if(offerPanel != null)
        {
            offerPanel.SetActive(true);
            offerPanel.transform.localScale = Vector3.zero;
            offerPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            if(startStreamButton) startStreamButton.interactable = false; 
        }
    }

    void OnOfferChoiceMade(bool accepted)
    {
        if(offerPanel != null) offerPanel.SetActive(false);

        if (accepted) GameManager.Instance.AcceptOffer(false); 
        else GameManager.Instance.PostponeOffer(false); 

        if (GameManager.Instance.morality > 0)
        {
            UpdateMainUI();
            StartCoroutine(AutoSpeakAtStart()); // Seçimden sonra konuşma
        }
    }

    void ReturnToRoom()
    {
        if(minigameObject != null) minigameObject.SetActive(false); 
        if(resultPanel != null) resultPanel.SetActive(false);
        if(streamUIManager != null) streamUIManager.EndStream();
        UpdateMainUI();
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
            liveViewerText.text = liveCount.ToString();
            
            liveViewerText.transform.DOKill();
            liveViewerText.transform.localScale = Vector3.one;
            liveViewerText.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
    }
}
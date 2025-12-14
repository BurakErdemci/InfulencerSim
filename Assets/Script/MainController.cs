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
    [SerializeField] private TrendHuntManager trendHuntManager;
    [SerializeField] private ChatModGameManager chatModManager; // Restored

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

    [Header("--- ROOM UI (CANLI VERİ) ---")]
    [SerializeField] private TextMeshProUGUI roomFollowerText; 
    [SerializeField] private TextMeshProUGUI roomSanityText;   

    [Header("--- GOD MODE PANELİ ---")]
    [SerializeField] private GameObject godModePanel;
    [SerializeField] private Button acceptGodModeButton;
    [SerializeField] private Button declineGodModeButton;

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
        
        if (startStreamButton != null)
        {
            startStreamButton.onClick.RemoveAllListeners();
            startStreamButton.onClick.AddListener(StartButtonLogic);
            
            // YENİ: Başlangıçta butonu kilitle, konuşma bitince açılsın
            startStreamButton.interactable = false;
        }

        if(continueButton != null) 
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnNextButtonPressed); 
        }

        // Faz 2 Butonları
        if (acceptOfferButton != null)
        {
            acceptOfferButton.onClick.RemoveAllListeners();
            acceptOfferButton.onClick.AddListener(() => OnOfferChoiceMade(true));
        }
        if (declineOfferButton != null)
        {
            declineOfferButton.onClick.RemoveAllListeners();
            declineOfferButton.onClick.AddListener(() => OnOfferChoiceMade(false));
        }
        
        // Faz 3 Butonları
        if (acceptGodModeButton != null)
        {
            acceptGodModeButton.onClick.RemoveAllListeners();
            acceptGodModeButton.onClick.AddListener(() => OnGodModeChoice(true));
        }
        if (declineGodModeButton != null)
        {
            declineGodModeButton.onClick.RemoveAllListeners();
            declineGodModeButton.onClick.AddListener(() => OnGodModeChoice(false));
        }

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
                float maxDarkness = 0.65f; 
                corruptionOverlay.color = new Color(0, 0, 0, ratio * maxDarkness);
            }
        }
    }

    IEnumerator AutoSpeakAtStart()
    {
        yield return new WaitForSeconds(1.0f);
        
        if(dialogueManager != null) 
        {
            // YENİ: Konuşma bitince Butonu AÇ (Callback)
            dialogueManager.SpeakInRoom(() => 
            {
                if (startStreamButton != null) startStreamButton.interactable = true;
            }); 
        }
        else
        {
            if (startStreamButton != null) startStreamButton.interactable = true;
        }
    }

    public void StartButtonLogic()
    {
        startStreamButton.interactable = false;
        if (trendHuntManager != null) trendHuntManager.StartTrendHunt();
        else OnTrendHuntFinished();
    }
    
    public void OnTrendHuntFinished()
    {
        UpdateMainUI(); 
        
        // Chain: TrendHunt -> ChatMod -> IntroSequence
        if(chatModManager != null)
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
        // Add score from ChatMod to stats
        float moralityEffect = (score > 500) ? 5f : -5f; // Example logic
        GameManager.Instance.UpdateStats(score, moralityEffect);
        
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        if(dialogueManager != null) dialogueManager.HideBubbleImmediately();
        CalculateLiveViewers();
        if(streamUIManager != null) streamUIManager.GoLive();
        
        yield return new WaitForSeconds(0.5f); 

        // MINIGAME HAZIRLIĞI
        if(minigameObject != null) 
        {
            minigameObject.SetActive(true); 
            if(minigameScript != null) 
            {
                minigameScript.SetupMinigame();
                // YENİ: Minigame Butonunu KİLİTLE
                minigameScript.SetStartButtonInteractable(false);
            }
        }

        // YENİ: Yayın Konuşması (Callback ile)
        if(dialogueManager != null) 
        {
            // Konuşma bitince Minigame Butonunu AÇ
            dialogueManager.SpeakInChat(() => 
            {
                if(minigameScript != null) minigameScript.SetStartButtonInteractable(true);
            });
        }
        else
        {
            if(minigameScript != null) minigameScript.SetStartButtonInteractable(true);
        }
    }

    public void CompleteStreamSession(int score)
    {
        if(minigameObject != null) minigameObject.SetActive(false);
        
        GameManager.Instance.ProcessMinigameEnd(score);
        ShowResults(score); 
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
                // YENİ: Odaya dönünce tekrar konuşma başlasın ve buton kilitlensin
                StartCoroutine(AutoSpeakAtStart());
            }
        }
        else
        {
            // YENİ: Teklif yoksa da konuşma başlasın
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
            startStreamButton.interactable = false;
        }
    }

    void OnGodModeChoice(bool accepted)
    {
        if(godModePanel != null) godModePanel.SetActive(false);

        if (accepted)
        {
            GameManager.Instance.AcceptOffer(true); 
            StartGlitchEffect(); 
        }
        else
        {
            GameManager.Instance.PostponeOffer(true); 
        }

        UpdateMainUI();
        StartCoroutine(AutoSpeakAtStart()); // Seçimden sonra konuşma
    }

    void OpenOfferPanel()
    {
        if(offerPanel != null)
        {
            offerPanel.SetActive(true);
            offerPanel.transform.localScale = Vector3.zero;
            offerPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            startStreamButton.interactable = false; 
        }
    }

    void OnOfferChoiceMade(bool accepted)
    {
        if(offerPanel != null) offerPanel.SetActive(false);

        if (accepted)
        {
            GameManager.Instance.AcceptOffer(false); 
        }
        else
        {
            GameManager.Instance.PostponeOffer(false); 
        }

        UpdateMainUI();
        StartCoroutine(AutoSpeakAtStart()); // Seçimden sonra konuşma
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

    void StartGlitchEffect()
    {
        if(mainFollowerText != null)
            mainFollowerText.transform.DOShakePosition(1f, 3f, 10, 90, false, true).SetLoops(-1);
        
        if(roomSanityText != null)
            roomSanityText.transform.DOShakePosition(0.5f, 5f, 20, 90, false, true).SetLoops(-1);

        if(startStreamButton != null)
            startStreamButton.transform.DOShakeScale(2f, 0.05f, 5, 90).SetLoops(-1);
    }
}
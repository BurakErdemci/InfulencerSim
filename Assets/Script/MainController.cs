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
        }

        if(continueButton != null) 
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnNextButtonPressed); 
        }

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
        }
    }

    IEnumerator AutoSpeakAtStart()
    {
        yield return new WaitForSeconds(1.0f);
        if(dialogueManager != null) dialogueManager.SpeakInRoom(); 
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
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        if(dialogueManager != null) dialogueManager.HideBubbleImmediately();
        CalculateLiveViewers();
        if(streamUIManager != null) streamUIManager.GoLive();
        
        yield return new WaitForSeconds(0.5f); 
        if(dialogueManager != null) dialogueManager.SpeakInChat();

        if(minigameObject != null) 
        {
            minigameObject.SetActive(true); 
            if(minigameScript != null) minigameScript.SetupMinigame();
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
                
                if (GameManager.Instance.morality > 50) resultSanityText.color = Color.green;
                else resultSanityText.color = Color.red;
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

        // Sıradaki hedefi geçtik mi?
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
                startStreamButton.interactable = true;
            }
        }
        else
        {
            startStreamButton.interactable = true;
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
        }
        else
        {
            // TRUE gönderiyoruz çünkü bu God Mode teklifi
            GameManager.Instance.PostponeOffer(true); 
        }

        UpdateMainUI();
        startStreamButton.interactable = true;
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
            // FALSE gönderiyoruz çünkü bu normal teklif
            GameManager.Instance.PostponeOffer(false); 
        }

        UpdateMainUI();
        startStreamButton.interactable = true;
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
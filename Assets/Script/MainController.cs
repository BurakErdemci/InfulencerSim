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

    [Header("--- ROOM UI (CANLI VERİ) ---")]
    [SerializeField] private TextMeshProUGUI roomFollowerText; 
    [SerializeField] private TextMeshProUGUI roomSanityText;   

    [Header("--- GOD MODE PANELİ ---")]
    [SerializeField] private GameObject godModePanel;
    [SerializeField] private Button acceptGodModeButton;
    [SerializeField] private Button declineGodModeButton;

    [Header("--- GAME OVER ---")]
    [SerializeField] private GameObject gameOverPanel; 
    [SerializeField] private Button returnToMenuButton; 

    [Header("--- GÖRSEL EFEKTLER (FX) ---")]
    [SerializeField] private Image discoOverlay;    // Faz 2: Renkli Panel (Disco)
    [SerializeField] private Image glitchOverlay;   // Faz 3: Bozuk Görüntü (Static Noise)
    [SerializeField] private AudioSource sfxSource; // Sesleri kalınlaştırmak için
    [SerializeField] private RectTransform[] shakingElements; // Titreyecek butonlar/yazılar
    
    // Scale Hafızası
    private Vector3 offerTargetScale;
    private Vector3 godModeTargetScale;
    private Vector3 gameOverTargetScale;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Müzik Kontrolü
        if (MusicManager.Instance != null) MusicManager.Instance.CheckAndPlayMusic();

        // Scale Hafızasını Al
        if (offerPanel != null) offerTargetScale = offerPanel.transform.localScale;
        if (godModePanel != null) godModeTargetScale = godModePanel.transform.localScale;
        if (gameOverPanel != null) gameOverTargetScale = gameOverPanel.transform.localScale;
        
        // UI Başlangıç Ayarları
        UpdateMainUI();
        if(resultPanel != null) resultPanel.SetActive(false);
        if(offerPanel != null) offerPanel.SetActive(false);
        if(godModePanel != null) godModePanel.SetActive(false);
        if(gameOverPanel != null) gameOverPanel.SetActive(false); 
        
        // Buton Dinleyicileri (Listeners)
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

        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(ReturnToMainMenu);
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
            
            // Eski corruption overlay (İstersen kaldırabilirsin, şimdilik dursun)
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
            gameOverPanel.transform.DOScale(gameOverTargetScale, 0.5f).SetEase(Ease.OutBack);
            
            startStreamButton.interactable = false;
            
            if (MusicManager.Instance != null) MusicManager.Instance.PlayGameOver();
        }
    }

    public void ReturnToMainMenu()
    {
        GameManager.Instance.ResetGameData();
        SceneManager.LoadScene("MainMenu");
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
            godModePanel.transform.DOScale(godModeTargetScale, 0.5f).SetEase(Ease.OutBack);
            startStreamButton.interactable = false;
        }
    }

    // --- GOD MODE SEÇİMİ ---
    void OnGodModeChoice(bool accepted)
    {
        if(godModePanel != null) godModePanel.SetActive(false);

        if (accepted)
        {
            GameManager.Instance.AcceptOffer(true); 
            if (MusicManager.Instance != null) MusicManager.Instance.CheckAndPlayMusic();
            
            // YENİ: Glitch Modunu Başlat!
            ActivateGlitchMode();
        }
        else
        {
            GameManager.Instance.PostponeOffer(true); 
        }

        if (GameManager.Instance.morality > 0)
        {
            UpdateMainUI();
            startStreamButton.interactable = true;
        }
    }

    void OpenOfferPanel()
    {
        if(offerPanel != null)
        {
            offerPanel.SetActive(true);
            offerPanel.transform.localScale = Vector3.zero;
            offerPanel.transform.DOScale(offerTargetScale, 0.5f).SetEase(Ease.OutBack);
            startStreamButton.interactable = false; 
        }
    }

    // --- İLK TEKLİF SEÇİMİ ---
    void OnOfferChoiceMade(bool accepted)
    {
        if(offerPanel != null) offerPanel.SetActive(false);

        if (accepted)
        {
            GameManager.Instance.AcceptOffer(false); 
            if (MusicManager.Instance != null) MusicManager.Instance.CheckAndPlayMusic();
            
            // YENİ: Disco Modunu Başlat!
            ActivateDiscoMode();
        }
        else
        {
            GameManager.Instance.PostponeOffer(false); 
        }

        if (GameManager.Instance.morality > 0)
        {
            UpdateMainUI();
            startStreamButton.interactable = true;
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



    // FAZ 2: DISCO MODU (Hafif Titreme + Renkli Işıklar)
    public void ActivateDiscoMode()
    {
        // 1. Ekran Rengi Dans Etsin
        if (discoOverlay != null)
        {
            discoOverlay.gameObject.SetActive(true);
            discoOverlay.color = new Color(1, 0, 1, 0.2f); // Pembe

            discoOverlay.DOColor(new Color(0, 1, 1, 0.2f), 0.5f) // Camgöbeğine dön
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear);
        }

        // 2. Kamera Hafif Sallansın (Ritim)
        if (Camera.main != null)
        {
            Camera.main.transform.DOShakePosition(1f, 0.3f, 5, 90, false, true).SetLoops(-1);
        }

        // 3. UI Elemanları Hafif Büyüyüp Küçülsün
        foreach (RectTransform rect in shakingElements)
        {
            if (rect != null)
            {
                rect.DOScale(1.05f, 0.4f).SetLoops(-1, LoopType.Yoyo);
            }
        }
    }

    // FAZ 3: GLITCH MODU (God Mode - Sistem Çöküşü)
    public void ActivateGlitchMode()
    {
        // Önce Disco modunu kapat
        if (discoOverlay != null) discoOverlay.gameObject.SetActive(false);
        Camera.main.transform.DOKill(); 

        // 1. Vahşi Kamera Sarsıntısı
        if (Camera.main != null)
        {
            Camera.main.transform.DOShakePosition(2f, 1.5f, 50, 90, false, true).SetLoops(-1);
            Camera.main.DOFieldOfView(50f, 0.1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0.5f);
        }

        // 2. Glitch Overlay (Yanıp Sönme / Noise)
        if (glitchOverlay != null)
        {
            glitchOverlay.gameObject.SetActive(true);
            
            Sequence glitchSeq = DOTween.Sequence();
            glitchSeq.Append(glitchOverlay.DOFade(0.8f, 0.05f)); 
            glitchSeq.Append(glitchOverlay.DOFade(0f, 0.05f));   
            glitchSeq.AppendInterval(0.1f);
            glitchSeq.Append(glitchOverlay.DOFade(0.6f, 0.02f)); 
            glitchSeq.Append(glitchOverlay.DOFade(0f, 0.02f));
            glitchSeq.SetLoops(-1); 
        }

        // 3. UI Elemanları Çıldırsın (Eski Shake logic + Stretch)
        foreach (RectTransform rect in shakingElements)
        {
            if (rect != null)
            {
                rect.DOKill(); 
                rect.DOShakeAnchorPos(1f, 15f, 50, 90, false, true).SetLoops(-1);
                rect.DOScale(new Vector3(1.2f, 0.8f, 1), 0.1f).SetLoops(-1, LoopType.Yoyo);
            }
        }

        // 4. Şeytani Ses Efekti (Buraya Taşıdık)
        if (sfxSource != null)
        {
            sfxSource.pitch = 0.6f; 
        }
    }
}
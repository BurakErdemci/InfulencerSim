using UnityEngine;
using UnityEngine.UI; 
using TMPro;          
using System.Collections;
using DG.Tweening; 

public class MainController : MonoBehaviour
{
    [Header("--- YÖNETİCİLER ---")]
    [SerializeField] private StreamUIManager streamUIManager; 
    [SerializeField] private DialogueManager dialogueManager;         

    [Header("--- ANA EKRAN UI ---")]
    [SerializeField] private TextMeshProUGUI mainFollowerText; 
    [SerializeField] private Button startStreamButton;         
    
    [Header("--- SONUÇ PANELI ---")]
    [SerializeField] private GameObject resultPanel;           
    [SerializeField] private TextMeshProUGUI resultGainText;   
    [SerializeField] private Button continueButton;            

    [Header("--- AYARLAR ---")]
    public float streamDuration = 5.0f; 

    void Start()
    {
        // 1. UI Güncelle
        UpdateMainUI();
        if(resultPanel != null) resultPanel.SetActive(false);
        
        // 2. Butonları Bağla
        if (startStreamButton != null)
        {
            startStreamButton.onClick.RemoveAllListeners();
            startStreamButton.onClick.AddListener(StartFullStreamSession);
        }

        if(continueButton != null) 
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(EndStreamSession);
        }
        
        // Oyun başlar başlamaz (otomatik) konuşma başlat
        StartCoroutine(AutoSpeakAtStart());
    }

    // Oyun açıldıktan 1 saniye sonra karakter konuşur
    IEnumerator AutoSpeakAtStart()
    {
        yield return new WaitForSeconds(1.0f); // Sahne yüklensin diye azıcık bekle
        if(dialogueManager != null) 
        {
            dialogueManager.SpeakInRoom(); 
        }
    }
    
    public void StartFullStreamSession()
    {
        startStreamButton.interactable = false;
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        // 1. Eğer baloncuk hala açıksa kapat (Temizlik)
        if(dialogueManager != null) dialogueManager.HideBubbleImmediately();

        // 2. Yayına Geçiş Yap
        if(streamUIManager != null) streamUIManager.GoLive();
        
        // 3. Geçiş Animasyonu Bekle
        yield return new WaitForSeconds(0.5f); 

        // 4. Yayın ekranında "Selam" desin
        if(dialogueManager != null) dialogueManager.SpeakInChat();

        // 5. Süre başlasın
        StartCoroutine(StreamDurationRoutine());
    }

    IEnumerator StreamDurationRoutine()
    {
        yield return new WaitForSeconds(streamDuration);

        int gainedFollowers = Random.Range(100, 500);
        float moralityLoss = 10f; 
        GameManager.Instance.UpdateStats(gainedFollowers, moralityLoss);

        OpenResultPanel(gainedFollowers);
    }

    void OpenResultPanel(int gain)
    {
        if(resultPanel != null)
        {
            resultPanel.SetActive(true);
            resultPanel.transform.localScale = Vector3.zero;
            resultPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            if(resultGainText != null) resultGainText.text = "+" + gain.ToString() + " Takipçi";
        }
    }

    public void EndStreamSession()
    {
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
}
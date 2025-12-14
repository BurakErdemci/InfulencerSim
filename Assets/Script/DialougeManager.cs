using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; 
using System.Collections;
using System; 

public class DialogueManager : MonoBehaviour
{
    [Header("Veri")]
    public DialogueData dialogueData; 

    [Header("UI Elemanları")]
    public GameObject dialoguePanel;      
    public Image portraitImage;           
    public TextMeshProUGUI dialogueText;  

    [Header("Ayarlar")]
    public float typingSpeed = 0.05f;     

    private Vector2 showPosition;
    private Vector2 hidePosition;

    void Start()
    {
        if(dialoguePanel != null) 
        {
            RectTransform rect = dialoguePanel.GetComponent<RectTransform>();
            showPosition = rect.anchoredPosition;
            // Aşağıya sakla
            hidePosition = new Vector2(showPosition.x, showPosition.y - rect.rect.height - 100);
            rect.anchoredPosition = hidePosition;
            dialoguePanel.SetActive(false);
        }
    }

    // --- ODA KONUŞMASI ---
    public void SpeakInRoom(Action onComplete = null)
    {
        // Eğer replik yoksa veya hata varsa direkt bitir ki oyun kilitlenmesin
        if (dialogueData == null || dialogueData.roomQuotes.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }
        string randomQuote = dialogueData.roomQuotes[UnityEngine.Random.Range(0, dialogueData.roomQuotes.Length)];
        StartCoroutine(TypewriterRoutine(randomQuote, onComplete));
    }

    // --- YAYIN KONUŞMASI ---
    public void SpeakInChat(Action onComplete = null)
    {
        if (dialogueData == null || dialogueData.chatQuotes.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }
        string randomQuote = dialogueData.chatQuotes[UnityEngine.Random.Range(0, dialogueData.chatQuotes.Length)];
        
        // Chat için de aynı paneli kullanıyoruz
        StartCoroutine(TypewriterRoutine(randomQuote, onComplete));
    }

    IEnumerator TypewriterRoutine(string textToType, Action onComplete)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = ""; 

        RectTransform rect = dialoguePanel.GetComponent<RectTransform>();
        rect.anchoredPosition = hidePosition; // Önce aşağı al
        rect.DOAnchorPos(showPosition, 0.5f).SetEase(Ease.OutBack); // Sonra yukarı çıkar

        foreach (char letter in textToType.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        float waitTime = dialogueData != null ? dialogueData.bubbleDuration : 3f;
        yield return new WaitForSeconds(waitTime);

        HidePanel();
        
        // İş bitti, MainController'a haber ver
        onComplete?.Invoke();
    }

    public void HidePanel()
    {
        if (dialoguePanel.activeSelf)
        {
            RectTransform rect = dialoguePanel.GetComponent<RectTransform>();
            rect.DOAnchorPos(hidePosition, 0.5f).OnComplete(() => 
            {
                dialoguePanel.SetActive(false);
            });
        }
    }
    
    public void HideBubbleImmediately()
    {
        if (dialoguePanel != null)
        {
            StopAllCoroutines();
            dialoguePanel.SetActive(false);
        }
    }
}
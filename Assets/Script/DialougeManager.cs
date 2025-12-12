using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; 
using System.Collections;

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

    [Header("Diğer")]
    public ChatManager chatManager;       
    
    private Vector2 showPosition;
   
    private Vector2 hidePosition;

    void Start()
    {
        if(dialoguePanel != null) 
        {
            
            RectTransform rect = dialoguePanel.GetComponent<RectTransform>();
            showPosition = rect.anchoredPosition;

            hidePosition = new Vector2(showPosition.x, showPosition.y - rect.rect.height - 50);
            
            rect.anchoredPosition = hidePosition;
            dialoguePanel.SetActive(false);
        }
    }

    // ODA KONUŞMASI
    public void SpeakInRoom()
    {
        if (dialogueData.roomQuotes.Length == 0) return;
        string randomQuote = dialogueData.roomQuotes[Random.Range(0, dialogueData.roomQuotes.Length)];
        
        StartCoroutine(TypewriterRoutine(randomQuote));
    }

    // YAYIN KONUŞMASI
    public void SpeakInChat()
    {
        if (dialogueData.chatQuotes.Length == 0) return;
        string randomQuote = dialogueData.chatQuotes[Random.Range(0, dialogueData.chatQuotes.Length)];
        if(chatManager != null) chatManager.SendStreamerMessage(randomQuote);
    }

    IEnumerator TypewriterRoutine(string textToType)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = ""; 

        RectTransform rect = dialoguePanel.GetComponent<RectTransform>();
        
        rect.anchoredPosition = hidePosition; 
        rect.DOAnchorPos(showPosition, 0.5f).SetEase(Ease.OutBack);

        foreach (char letter in textToType.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        float waitTime = dialogueData != null ? dialogueData.bubbleDuration : 3f;
        yield return new WaitForSeconds(waitTime);

        HidePanel();
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
using UnityEngine;
using TMPro;
using DG.Tweening; 
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("Veri")]
    public DialogueData dialogueData; 

    [Header("Balon Ayarları")]
    public GameObject speechBubble;       // Balonun kendisi (Hiyerarşideki Dialog -> Dialoguebubble)
    public TextMeshProUGUI bubbleText;    // İçindeki yazı
    
    [Header("Konum Hedefleri (Yeni)")]
    public Transform roomTarget;    // Oda sahnesindeki kafa üstü noktası
    public Transform streamTarget;  // Yayın sahnesindeki facecam noktası

    [Header("Diğer")]
    public ChatManager chatManager;       

    private Vector3 originalScale;

    void Start()
    {
        if(speechBubble != null) 
        {
            originalScale = speechBubble.transform.localScale;
            speechBubble.SetActive(false);
            speechBubble.transform.localScale = Vector3.zero;
        }
    }

    // ODA SAHNESİNDE KONUŞMA
    public void SpeakInRoom()
    {
        // 1. Balonu Oda konumuna ışınla
        if (roomTarget != null) 
            speechBubble.transform.position = roomTarget.position;

        // 2. Metni seç
        if (dialogueData.roomQuotes.Length > 0)
        {
            string quote = dialogueData.roomQuotes[Random.Range(0, dialogueData.roomQuotes.Length)];
            ShowBubble(quote);
        }
    }
    
    public void SpeakInChat()
    {
        // 1. Balonu Yayın (Facecam) konumuna ışınla
        if (streamTarget != null) 
            speechBubble.transform.position = streamTarget.position;

        // 2. Metni seç
        if (dialogueData.chatQuotes.Length > 0)
        {
            string quote = dialogueData.chatQuotes[Random.Range(0, dialogueData.chatQuotes.Length)];
            
           
            ShowBubble(quote);
            
            if(chatManager != null) chatManager.SendStreamerMessage(quote);
        }
    }

    // ORTAK BALON AÇMA FONKSİYONU
    private void ShowBubble(string text)
    {
        speechBubble.SetActive(true);
        bubbleText.text = text;
        
        speechBubble.transform.localScale = Vector3.zero;
        speechBubble.transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutBack);
        
        float duration = dialogueData != null ? dialogueData.bubbleDuration : 3f;
        StartCoroutine(CloseBubbleAfterTime(duration));
    }

    public void HideBubbleImmediately()
    {
        if (speechBubble != null)
        {
            speechBubble.transform.DOKill();
            speechBubble.SetActive(false);
        }
    }

    IEnumerator CloseBubbleAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        speechBubble.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() => speechBubble.SetActive(false));
    }
}
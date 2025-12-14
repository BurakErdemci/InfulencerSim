using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class ChatManager : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public Transform chatContent;       
    public GameObject textPrefab;       
    public ScrollRect scrollRect;      

    [Header("Ayarlar")]
    public float messageSpeed = 2f;   

    private string[] goodMessages = { "Harikasın!", "Kraliçe <3", "Çok tatlısın", "Selamlar!", "Oha yeteneğe bak", "Kalp Kalp Kalp" };
    private string[] badMessages = { "SATILMIŞ!", "Paragöz", "Bunu yapma", "Unfollow", "Dislike", "Eski halin iyiydi", "BOŞ YAPMA" };
    private string[] botMessages = { "FREE CRYPTO CLICK HERE", "WIN IPHONE 15", "$$$ Money $$$", "Hot Singles Area" };

    private bool isStreaming = false;
    private float timer;

    public void StartChat()
    {
        foreach (Transform child in chatContent) Destroy(child.gameObject);
        isStreaming = true;
    }

    public void StopChat()
    {
        isStreaming = false;
    }

    void Update()
    {
        if (!isStreaming) return;

        timer += Time.deltaTime;
        if (timer >= messageSpeed)
        {
            SpawnMessage();
            timer = 0f;
        }
    }

    void SpawnMessage()
    {
        string message = "";
        float morality = GameManager.Instance.morality;
        
        // Akıl sağlığına göre mesaj havuzu seç
        if (morality > 70) 
            message = goodMessages[Random.Range(0, goodMessages.Length)];
        else if (morality > 30) 
            message = Random.value > 0.5f ? goodMessages[Random.Range(0, goodMessages.Length)] : badMessages[Random.Range(0, badMessages.Length)];
        else 
            message = Random.value > 0.3f ? badMessages[Random.Range(0, badMessages.Length)] : botMessages[Random.Range(0, botMessages.Length)];
        
        GameObject newText = Instantiate(textPrefab, chatContent);
        TextMeshProUGUI textComp = newText.GetComponent<TextMeshProUGUI>();
        textComp.text = "<b>User" + Random.Range(100, 999) + ":</b> " + message;
        
        if (morality < 30 && (message.Contains("SATILMIŞ") || message.Contains("Dislike")))
            textComp.color = Color.red;
        else
            textComp.color = Color.whiteSmoke;
        
        Canvas.ForceUpdateCanvases();
        if(scrollRect != null) scrollRect.DOVerticalNormalizedPos(0f, 0.3f);
    }

   
    public void SendStreamerMessage(string message)
    {
        // 1. Obje Yarat
        GameObject newText = Instantiate(textPrefab, chatContent);
        
        // 2. Text Ayarla (Yayıncı olduğu belli olsun diye Sarı renk)
        TextMeshProUGUI textComp = newText.GetComponent<TextMeshProUGUI>();
        textComp.text = "<b><color=yellow>STREAMER:</color></b> " + message; 
        textComp.color = Color.yellow; 
        
        // 3. Scroll Aşağı İndir
        Canvas.ForceUpdateCanvases();
        if(scrollRect != null) 
        {
            scrollRect.DOVerticalNormalizedPos(0f, 0.3f);
        }
    }
}
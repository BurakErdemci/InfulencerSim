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
        // Önceki mesajları temizle
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
        // God Mode'da chat daha hızlı aksın (Kaos hissi için)
        float currentSpeed = (GameManager.Instance != null && GameManager.Instance.isGodMode) ? messageSpeed * 0.5f : messageSpeed;

        if (timer >= currentSpeed)
        {
            SpawnMessage();
            timer = 0f;
        }
    }

    void SpawnMessage()
    {
        string content = "";
        float morality = GameManager.Instance != null ? GameManager.Instance.morality : 100;
        
        // --- 1. MESAJ İÇERİĞİNİ SEÇ ---
        if (morality > 70) 
            content = goodMessages[Random.Range(0, goodMessages.Length)];
        else if (morality > 30) 
            content = Random.value > 0.5f ? goodMessages[Random.Range(0, goodMessages.Length)] : badMessages[Random.Range(0, badMessages.Length)];
        else 
            content = Random.value > 0.3f ? badMessages[Random.Range(0, badMessages.Length)] : botMessages[Random.Range(0, botMessages.Length)];
        
        // --- 2. OBJEYİ YARAT ---
        GameObject newText = Instantiate(textPrefab, chatContent);
        TextMeshProUGUI textComp = newText.GetComponent<TextMeshProUGUI>();
        
        // Kullanıcı adını oluştur
        string fullMessage = "<b>User" + Random.Range(100, 999) + ":</b> " + content;

        // --- 3. STİL VE EFEKTLERİ UYGULA (BURASI YENİ) ---
        ApplyChatStyle(textComp, fullMessage);
        
        // --- 4. SCROLL VE GÜNCELLEME ---
        Canvas.ForceUpdateCanvases();
        if(scrollRect != null) scrollRect.DOVerticalNormalizedPos(0f, 0.3f);
    }

    public void SendStreamerMessage(string message)
    {
        GameObject newText = Instantiate(textPrefab, chatContent);
        TextMeshProUGUI textComp = newText.GetComponent<TextMeshProUGUI>();
        
        // Yayıncı mesajı da God Mode'da hafif kırılsın ama okunabilsin
        if (GameManager.Instance != null && GameManager.Instance.isGodMode)
        {
            textComp.text = "<b><color=red>STR€AMER:</color></b> " + message; 
            textComp.color = new Color(1f, 0.8f, 0.8f); // Soluk kırmızımsı sarı
        }
        else
        {
            textComp.text = "<b><color=yellow>STREAMER:</color></b> " + message; 
            textComp.color = Color.yellow; 
        }
        
        Canvas.ForceUpdateCanvases();
        if(scrollRect != null) 
        {
            scrollRect.DOVerticalNormalizedPos(0f, 0.3f);
        }
    }


    void ApplyChatStyle(TextMeshProUGUI textComponent, string originalMessage)
    {
        if (GameManager.Instance == null) return;

        // --- FAZ 3: GOD MODE (Karanlık & Glitch) ---
        if (GameManager.Instance.isGodMode)
        {
            // Renk: Kan Kırmızısı / Koyu Bordo
            textComponent.color = new Color(Random.Range(0.6f, 1f), 0f, 0f); 
            
            // Font: Kalın ve İtalik (Agresif)
            textComponent.fontStyle = FontStyles.Bold | FontStyles.Italic;

            // Metni Boz (Glitch Efekti)
            string corruptedText = "";
            string scarySymbols = "¡¢£¤¥¦§¨©ª«¬®¯°±²³´µ¶·¸¹º»¼½¾¿×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";
            
            // Orijinal mesajı harf harf gez
            foreach (char c in originalMessage)
            {
                // HTML taglerine (<b> vs) dokunma, yoksa kod bozulur
                if (c == '<' || c == '>' || c == '/') 
                {
                    corruptedText += c;
                    continue; 
                }

                // %25 ihtimalle harfi boz
                if (Random.value < 0.25f) 
                {
                    corruptedText += scarySymbols[Random.Range(0, scarySymbols.Length)];
                }
                else
                {
                    corruptedText += c;
                }
            }
            textComponent.text = corruptedText;
        }
        // --- FAZ 2: DISCO MODU (Renkli & Enerjik) ---
        else if (GameManager.Instance.isCorrupt)
        {
            // Renk: Rastgele Parlak Neon Renkler
            textComponent.color = Color.HSVToRGB(Random.value, 0.8f, 1f); // Yüksek Saturation ve Value
            
            // Metin: Ünlem ekle
            textComponent.text = originalMessage + "!!!";
            textComponent.fontStyle = FontStyles.Normal;
        }
        // --- FAZ 1: NORMAL MOD ---
        else
        {
            // Eski "Satılmış/Dislike" mantığını koruyalım (kırmızı yapıyordun)
            if (originalMessage.Contains("SATILMIŞ") || originalMessage.Contains("Dislike"))
                textComponent.color = Color.red;
            else
                textComponent.color = Color.whiteSmoke;

            textComponent.text = originalMessage;
            textComponent.fontStyle = FontStyles.Normal;
        }
    }
}
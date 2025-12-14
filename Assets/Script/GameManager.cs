using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("--- OYUN DURUMU ---")]
    public long followers = 100;    
    public float morality = 100f;   
    public int streamCount = 0;     

    [Header("--- KILITLER VE FAZLAR ---")]
    public bool isCorrupt = false;      
    public bool isGodMode = false;      
    
    public bool offerPresented = false;        
    public bool godModeOfferPresented = false; 

    [Header("--- OTOMATİK TEKLİF SİSTEMİ ---")]
    public long nextEventThreshold = 5000; 
    public long offerInterval = 5000;      

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // --- YENİ: VERİLERİ SIFIRLAMA FONKSİYONU ---
    public void ResetGameData()
    {
        followers = 100;
        morality = 100f;
        streamCount = 0;

        isCorrupt = false;
        isGodMode = false;
        offerPresented = false;
        godModeOfferPresented = false;

        nextEventThreshold = 5000;
        
        Debug.Log("Oyun verileri sıfırlandı.");
    }

    public void UpdateGeneralStats(int followerGain, float sanityChange)
    {
        followers += followerGain;
        morality += sanityChange;
        morality = Mathf.Clamp(morality, 0f, 100f);

        // TrendHunt sırasında ölürse kontrolü
        CheckGameOver();
    }

    public void ProcessMinigameEnd(int rawScore)
    {
        // 1. Çarpan Hesabı
        float multiplier = 1.0f;
        if (isGodMode) multiplier = 6.0f;
        else if (isCorrupt) multiplier = 3.0f;
        else if (offerPresented && !isCorrupt) multiplier = 1.2f; 
        else multiplier = 1.0f; 

        // 2. Takipçi Hesabı
        long gain = (long)(rawScore * multiplier);
        followers += gain;

        // 3. Akıl Sağlığı
        float sanityChange = 0;
        if (isCorrupt || isGodMode)
        {
            if (gain > 2000) sanityChange = -5f;  
            else sanityChange = -15f; 
        }
        else
        {
            if (gain > 1000) sanityChange = 10f; 
            else sanityChange = -5f; 
        }

        morality += sanityChange;
        morality = Mathf.Clamp(morality, 0f, 100f);
        streamCount++;
        
        Debug.Log($"Yayın Bitti. Takipçi: {followers} / Sıradaki Hedef: {nextEventThreshold}");

        // --- YENİ: GAME OVER KONTROLÜ ---
        // Eğer morality bittiyse MainController'ı tetikle ve çık
        if (CheckGameOver()) return; 
    }

    // Yardımcı Fonksiyon: Ölümü kontrol eder
    private bool CheckGameOver()
    {
        if (morality <= 0)
        {
            Debug.Log("GAME OVER! Akıl sağlığı tükendi.");
            if (MainController.Instance != null)
                MainController.Instance.TriggerGameOver();
            return true; // Oyun bitti
        }
        return false; // Devam
    }

    public void PostponeOffer(bool isGodModeOffer)
    {
        nextEventThreshold += offerInterval;
        if (isGodModeOffer) godModeOfferPresented = true;
        else offerPresented = true;
        Debug.Log($"Teklif reddedildi. Bir sonraki: {nextEventThreshold}");
    }

    public void AcceptOffer(bool isGodModeDeal)
    {
        if (isGodModeDeal)
        {
            isGodMode = true;
            godModeOfferPresented = true; 
            morality -= 30f; 
            nextEventThreshold += 9999999; 
        }
        else
        {
            isCorrupt = true;
            offerPresented = true; 
            morality -= 20f;
            nextEventThreshold += offerInterval;
        }
        morality = Mathf.Clamp(morality, 0f, 100f);
        
        // Kabul edince morality düşüyor, burada da ölebilir
        CheckGameOver();
    }
}
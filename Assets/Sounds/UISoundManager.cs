using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance;

    [Header("Ses Dosyaları")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Ton Ayarları")]
    [Range(0.8f, 1.2f)] public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.1f;

    private AudioSource hoverSource;
    private AudioSource clickSource;

    // --- YENİ EKLENEN KISIM: ZAMANLAYICI ---
    private float lastHoverTime = 0f; // Son çalınma zamanını tutar
    private float hoverCooldown = 0.15f; // İki ses arası en az kaç saniye geçmeli?

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        hoverSource = gameObject.AddComponent<AudioSource>();
        clickSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayHover()
    {
        // --- KRİTİK KONTROL ---
        // Eğer şu anki zaman, son çalınma zamanından çok yakınsa çalma!
        if (Time.time - lastHoverTime < hoverCooldown) return;

        // Zamanlayıcıyı güncelle
        lastHoverTime = Time.time;

        hoverSource.Stop(); 
        
        if (hoverSound != null) 
        {
            hoverSource.pitch = Random.Range(minPitch, maxPitch);
            hoverSource.PlayOneShot(hoverSound);
        }
        if (hoverSource.isPlaying) return;

        // 2. KURAL: Ses dosyan yoksa hata verme, dön.
        if (hoverSound == null) return;

        // --- Ton Ayarı ---
        hoverSource.pitch = Random.Range(minPitch, maxPitch);
        
        // --- Sesi Çal ---
        // PlayOneShot yerine Play kullanıyoruz ki isPlaying doğru çalışsın.
        // Ama PlayOneShot da olur, isPlaying onu da algılar.
        hoverSource.PlayOneShot(hoverSound);
    }

    public void PlayClick()
    {
        hoverSource.Stop(); 

        if (clickSound != null) 
        {
            clickSource.pitch = Random.Range(minPitch, maxPitch);
            clickSource.PlayOneShot(clickSound);
        }
    }
}
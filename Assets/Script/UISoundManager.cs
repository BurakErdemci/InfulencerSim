using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance;

    [Header("Ses Dosyaları")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    // İki ayrı hoparlörümüz (AudioSource) olacak
    private AudioSource hoverSource;
    private AudioSource clickSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Hover sesleri için özel kaynak yarat
        hoverSource = gameObject.AddComponent<AudioSource>();
        hoverSource.playOnAwake = false;

        // Click sesleri için özel kaynak yarat
        clickSource = gameObject.AddComponent<AudioSource>();
        clickSource.playOnAwake = false;
    }

    public void PlayHover()
    {
        // 1. SORUN ÇÖZÜMÜ: Çakışma engelleme
        // Eğer şu an bir hover sesi çalıyorsa, onu sustur, yenisini çal.
        // Böylece "vıj vıj vıj" diye üst üste binmez, tek ve net duyulur.
        hoverSource.Stop(); 
        
        if (hoverSound != null) 
            hoverSource.PlayOneShot(hoverSound); // Ses seviyesini buradan kısabilirsin: (hoverSound, 0.5f)
    }

    public void PlayClick()
    {
        // 2. SORUN ÇÖZÜMÜ: Tıklayınca hover sussun
        // Tıkladığımız an hover sesini bıçak gibi kesiyoruz.
        hoverSource.Stop(); 

        if (clickSound != null) 
            clickSource.PlayOneShot(clickSound);
    }
}
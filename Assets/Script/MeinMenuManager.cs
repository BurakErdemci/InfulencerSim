using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI resmini değiştirmek için gerekli

public class MainMenuManager : MonoBehaviour
{
    [Header("Sahne Ayarları")]
    public string gameSceneName = "Tutorial";

    [Header("Paneller")]
    public GameObject optionsPanel;
    public GameObject mainMenuPanel;

    [Header("Ses Ayarları")]
    public Image soundButtonImage; // Butonun üzerindeki resim (Image component)
    public Sprite soundOnSprite;   // Ses AÇIK iken görünecek ikon
    public Sprite soundOffSprite;  // Ses KAPALI iken görünecek ikon

    private bool isMuted = false; // Sesin durumu

    void Start()
    {
        // Oyun açıldığında ses ayarını kontrol et (Varsayılan: Açık)
        UpdateSoundIcon();
    }
    void Update()
    {
        // ESC tuşuna basılınca ne olsun?
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf)
            {
                // Eğer panel açıksa kapat
                CloseOptions(); 
            }
            else
            {
                // Eğer panel kapalıysa aç (İstersen bunu sil, ESC genelde sadece kapatır)
                OpenOptions();
            }
        }
    }
    

    public void PlayGame()
    {
        Debug.Log("Butona tıklandı! Sahne yüklenmeye çalışılıyor..."); // Konsola mesaj yazdırır
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // --- SES KAPATMA / AÇMA (MUTE) ---
    public void ToggleSound()
    {
        isMuted = !isMuted; // Durumu tam tersine çevir (Açıksa kapat, kapalıysa aç)

        if (isMuted)
        {
            AudioListener.volume = 0; // Sesi tamamen kes
        }
        else
        {
            AudioListener.volume = 1; // Sesi aç
        }

        UpdateSoundIcon(); // İkonu güncelle
    }

    // İkonu duruma göre değiştiren yardımcı fonksiyon
    void UpdateSoundIcon()
    {
        if (soundButtonImage != null)
        {
            // Eğer sessizdeysek 'Off' resmi, değilse 'On' resmi koy
            soundButtonImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
        }
    }
}
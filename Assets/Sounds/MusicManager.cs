using UnityEngine;
using UnityEngine.SceneManagement; 

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("--- SES KAYNAĞI ---")]
    public AudioSource audioSource;

    [Header("--- MENÜ VE TUTORIAL (FAZ 0) ---")]
    public AudioClip mainMenuTheme;   // Ana Menü ve Tutorial'da bu çalacak

    [Header("--- OYUN İÇİ FAZLAR ---")]
    public AudioClip phase1Normal;    // Oyun Başlangıcı
    public AudioClip phase2Corrupt;   // Teklif sonrası
    public AudioClip phase3GodMode;   // God Mode
    
    [Header("--- SONUÇ ---")]
    public AudioClip gameOverTheme;   

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- YENİ: OTOMATİK SAHNE TAKİP SİSTEMİ ---
    // Bu iki fonksiyon, sahne değiştiği an Unity'nin bize haber vermesini sağlar.
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Sahne her değiştiğinde burası otomatik çalışır!
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndPlayMusic();
    }
    // -------------------------------------------

    public void CheckAndPlayMusic()
    {
        AudioClip targetClip = null;
        string currentScene = SceneManager.GetActiveScene().name;

    
        if (currentScene == "mainMenu" || currentScene == "Tutorial")
        {
            targetClip = mainMenuTheme;
        }
        else
        {
            // Bu iki sahne dışındaysak (yani asıl oyundaysak) Fazlara bak
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.isGodMode)
                {
                    targetClip = phase3GodMode;
                }
                else if (GameManager.Instance.isCorrupt)
                {
                    targetClip = phase2Corrupt;
                }
                else
                {
                    targetClip = phase1Normal;
                }
            }
            else
            {
                // GameManager yoksa ve oyun sahnesindeysek normal çal
                targetClip = phase1Normal;
            }
        }

        PlayClip(targetClip);
    }

    public void PlayGameOver()
    {
        // Game Over müziği loop olmasın, bir kere çalsın ve dursun dersen loop = false yap.
        PlayClip(gameOverTheme);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        
        // Aynı müzik zaten çalıyorsa kesinti yapma (Smooth geçiş)
        if (audioSource.clip == clip && audioSource.isPlaying) return;

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
    
}
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;
    void Awake() { Instance = this; }

    [Header("Prefab Ayarları")]
    public GameObject playerPrefab;
    public GameObject[] goodItems;
    public GameObject[] badItems;
    
    [Header("--- SPECIAL ITEMS ---")]
    public GameObject moneyBagPrefab; 
    [Range(0f, 1f)] public float moneyBagChance = 0.25f; 

    [Header("Spawn Hızı")]
    public float normalSpawnInterval = 0.45f;   
    public float corruptSpawnInterval = 0.6f; // Yozlaşınca oyun kolaylaşsın (daha yavaş spawn)
    public float godModeSpawnInterval = 0.1f; // God mode kaosu

    [Header("UI Elementleri")]
    public Button startGameButton;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameBackground;
    public RectTransform gameArea;

    [Header("Oyun Ayarları")]
    public float gameDuration = 20f;
    public float objectScale = 1f;

    [Header("Zorluk Ayarları")]
    [SerializeField] private float rampDuration = 30f;
    [SerializeField] private float startSpeedMult = 2.2f;
    [SerializeField] private float endSpeedMult = 6.0f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0,0,1,1);
    [SerializeField] private float corruptSpeedMult = 0.5f; // Yozlaşınca düşüş hızı yarıya insin

    [Header("Ses")]
    public AudioSource sfxSource;
    public AudioClip collectClip;
    public AudioClip badClip;
    public AudioClip vacuumLoopClip;
    private float lastSoundTime;

    // FallingObject'in eriştiği hız çarpanı
    public float CurrentSpeedMultiplier { get; private set; } = 1f;
    
    // FallingObject'in erişmesi gereken değişkenler:
    public bool IsVacuumActive => GameManager.Instance != null && GameManager.Instance.isGodMode;
    public float vacuumFallMult = 0.05f;    
    public float vacuumPullSpeed = 2800f;   
    public float vacuumCatchDistance = 55f; 

    private bool isPlaying = false;
    private RectTransform currentPlayer;
    private int currentScore = 0;
    private float gameStartTime;

    public RectTransform PlayerRT => currentPlayer;

    void Update()
    {
        if (!isPlaying || GameManager.Instance == null) return;

        // ---- HIZ KONTROLÜ (FAZLARA GÖRE) ----
        
        // DURUM 1: GOD MODE (Vakum açık, hız önemsiz ama akış hızlı olsun)
        if (GameManager.Instance.isGodMode)
        {
            // God Mode'da düşüş hızı normalin biraz üstü kalsın, vakum çekecek zaten
            CurrentSpeedMultiplier = startSpeedMult; 
        }
        // DURUM 2: CORRUPT (Kolaylaştırılmış Mod - Para Çantaları)
        else if (GameManager.Instance.isCorrupt)
        {
            // Hız sabit ve yavaş (Oyuncu rahatça toplasın)
            CurrentSpeedMultiplier = startSpeedMult * corruptSpeedMult;
        }
        // DURUM 3: NORMAL (Zorlanan Oyuncu)
        else
        {
            // Zamanla hızlanan (Ramp) zorluk eğrisi
            float t = (Time.time - gameStartTime) / Mathf.Max(0.01f, rampDuration);
            t = Mathf.Clamp01(t);
            float eased = speedCurve.Evaluate(t);
            
            CurrentSpeedMultiplier = Mathf.Lerp(startSpeedMult, endSpeedMult, eased);
        }
    }

    public void SetupMinigame()
    {
        Cleanup();
        currentScore = 0;
        if (scoreText) scoreText.text = "Skor: 0";
        if (timerText) timerText.text = Mathf.CeilToInt(gameDuration).ToString();

        if (startGameButton) startGameButton.gameObject.SetActive(true);
        if (gameBackground) gameBackground.SetActive(false);

        if (startGameButton) {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(StartMinigameLogic);
        }
    }

    void StartMinigameLogic()
    {
        if (startGameButton) startGameButton.gameObject.SetActive(false);
        if (gameBackground) gameBackground.SetActive(true);

        isPlaying = true;
        gameStartTime = Time.time;

        // God Mode ise Vakum sesi çal
        if (IsVacuumActive && sfxSource && vacuumLoopClip)
        {
            sfxSource.clip = vacuumLoopClip;
            sfxSource.loop = true;
            sfxSource.Play();
        }

        SpawnPlayer();
        StartCoroutine(SpawnRoutine());
        StartCoroutine(GameTimerRoutine());
    }

    void SpawnPlayer()
    {
        if (currentPlayer != null) Destroy(currentPlayer.gameObject);
        Transform parent = (gameArea != null) ? gameArea : transform;
        GameObject p = Instantiate(playerPrefab, parent);
        p.tag = "Player";
        currentPlayer = p.GetComponent<RectTransform>();
        if (currentPlayer != null && gameArea != null) {
            float y = -(gameArea.rect.height * 0.5f) + 60f;
            currentPlayer.anchoredPosition = new Vector2(0, y);
            currentPlayer.localScale = Vector3.one;
            currentPlayer.localRotation = Quaternion.identity;
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (isPlaying) {
            SpawnItem();
            
            // Spawn hızını faza göre belirle
            float waitTime = normalSpawnInterval;
            
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.isGodMode) waitTime = godModeSpawnInterval; // Çılgın atış
                else if (GameManager.Instance.isCorrupt) waitTime = corruptSpawnInterval; // Daha rahat
            }
            
            yield return new WaitForSeconds(waitTime);
        }
    }

    void SpawnItem()
    {
        if (gameArea == null) return;
        GameObject prefabToSpawn = null;
        
        bool isCorrupt = GameManager.Instance != null && GameManager.Instance.isCorrupt;
        bool isGod = GameManager.Instance != null && GameManager.Instance.isGodMode;

        // Para Çantası Mantığı (Sadece Yozlaşmış veya God Mode ise)
        if ((isCorrupt || isGod) && moneyBagPrefab != null) {
            if (Random.value <= moneyBagChance) prefabToSpawn = moneyBagPrefab;
        }

        // Eğer para çantası gelmediyse normal/kötü obje seç
        if (prefabToSpawn == null) {
            float goodChance = 0.6f; 
            if (isCorrupt) goodChance = 0.85f; // Yozlaşınca iyi gelme şansı artsın
            if (isGod) goodChance = 0.95f;     // God mode neredeyse hep iyi
            
            GameObject[] pool = (Random.value < goodChance) ? goodItems : badItems;
            if (pool != null && pool.Length > 0) prefabToSpawn = pool[Random.Range(0, pool.Length)];
        }

        if (prefabToSpawn != null) {
            GameObject obj = Instantiate(prefabToSpawn, gameArea);
            
            // UI üzerinde render sırası için (Önde görünsün)
            Canvas canvas = obj.GetComponent<Canvas>();
            if(canvas) { canvas.overrideSorting = true; canvas.sortingOrder = 10; }
            
            // Eğer SpriteRenderer varsa (UI değilse)
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if(sr) sr.sortingOrder = 10;

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null) {
                float xRange = (gameArea.rect.width * 0.5f) - 60f;
                float y = (gameArea.rect.height * 0.5f) + 40f;
                rt.anchoredPosition3D = new Vector3(Random.Range(-xRange, xRange), y, 0f);
                rt.localScale = prefabToSpawn.transform.localScale * objectScale;
                rt.localRotation = Quaternion.identity;
            }
        }
    }

    public void AddScore(int val)
    {
        currentScore += val;
        if (scoreText) {
            scoreText.text = "Skor: " + currentScore;
            scoreText.transform.DOKill(); 
            scoreText.transform.localScale = Vector3.one; 
            scoreText.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 1, 0.5f);
        }
        PlayCollectSound(val < 0);
    }

    public void PlayCollectSound(bool isBad)
    {
        // God mode'da vakum sesi var, pıt pıt sesine gerek yok kafa şişirmesin
        if (IsVacuumActive) return; 
        
        if (Time.time < lastSoundTime + 0.08f) return;
        if (sfxSource) {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            if (isBad && badClip) sfxSource.PlayOneShot(badClip);
            else if (collectClip) sfxSource.PlayOneShot(collectClip);
        }
        lastSoundTime = Time.time;
    }

    IEnumerator GameTimerRoutine()
    {
        float time = gameDuration;
        while (time > 0 && isPlaying) {
            if (timerText) timerText.text = Mathf.CeilToInt(time).ToString();
            yield return new WaitForSeconds(1f);
            time--;
        }
        EndMinigame();
    }

    public void EndMinigame()
    {
        isPlaying = false;
        
        // Loop sesi durdur
        if (sfxSource && sfxSource.loop) sfxSource.Stop();

        Cleanup();
        if (MainController.Instance != null) MainController.Instance.CompleteStreamSession(currentScore);
    }

    void Cleanup()
    {
        StopAllCoroutines();
        if (currentPlayer != null) { Destroy(currentPlayer.gameObject); currentPlayer = null; }
        if (gameArea != null) {
            foreach (var f in gameArea.GetComponentsInChildren<FallingObject>(true)) Destroy(f.gameObject);
        }
    }

    // --- MAIN CONTROLLER'IN KULLANDIĞI KİLİT FONKSİYONU ---
    public void SetStartButtonInteractable(bool state)
    {
        if (startGameButton != null)
        {
            startGameButton.interactable = state;
            // Butonun görünürlüğünü de hafif kıs ki kilitli olduğu anlaşılsın
            CanvasGroup cg = startGameButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = startGameButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = state ? 1f : 0.5f;
        }
    }
}
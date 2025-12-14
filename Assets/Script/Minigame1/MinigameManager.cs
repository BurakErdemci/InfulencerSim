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
    public float corruptSpawnInterval = 0.15f;  

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
    [SerializeField] private float corruptSpeedMult = 0.45f;

    [Header("Ses")]
    public AudioSource sfxSource;
    public AudioClip collectClip;
    public AudioClip badClip;
    public AudioClip vacuumLoopClip;
    private float lastSoundTime;

    public float CurrentSpeedMultiplier { get; private set; } = 1f;
    
    // FallingObject'in erişmesi gereken değişkenler:
    public bool IsVacuumActive => GameManager.Instance.isGodMode;
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
        if (!isPlaying) return;

        // ---- HIZ KONTROLÜ ----
        if (GameManager.Instance.isCorrupt)
        {
            CurrentSpeedMultiplier = startSpeedMult * corruptSpeedMult;
        }
        else
        {
            float t = (Time.time - gameStartTime) / Mathf.Max(0.01f, rampDuration);
            t = Mathf.Clamp01(t);
            float eased = speedCurve.Evaluate(t);
            
            // DÜZELTME 1: offerMade -> offerPresented
            float difficultyOffset = GameManager.Instance.offerPresented ? 0.5f : 0f;
            
            // DÜZELTME 2: godModeOfferMade -> godModeOfferPresented
            if (GameManager.Instance.godModeOfferPresented && !GameManager.Instance.isGodMode)
            {
                difficultyOffset += 1.5f; 
            }
            
            CurrentSpeedMultiplier = Mathf.Lerp(startSpeedMult, endSpeedMult, eased) + difficultyOffset;
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
            float waitTime = normalSpawnInterval;
            if (GameManager.Instance.isGodMode) waitTime = 0.1f; 
            else if (GameManager.Instance.isCorrupt) waitTime = corruptSpawnInterval; 
            yield return new WaitForSeconds(waitTime);
        }
    }

    void SpawnItem()
    {
        if (gameArea == null) return;
        GameObject prefabToSpawn = null;
        bool isCorruptOrGod = GameManager.Instance.isCorrupt || GameManager.Instance.isGodMode;

        if (isCorruptOrGod && moneyBagPrefab != null) {
            if (Random.value <= moneyBagChance) prefabToSpawn = moneyBagPrefab;
        }

        if (prefabToSpawn == null) {
            float goodChance = 0.6f; 
            if (isCorruptOrGod) goodChance = 0.90f; 
            GameObject[] pool = (Random.value < goodChance) ? goodItems : badItems;
            if (pool != null && pool.Length > 0) prefabToSpawn = pool[Random.Range(0, pool.Length)];
        }

        if (prefabToSpawn != null) {
            GameObject obj = Instantiate(prefabToSpawn, gameArea);
            
            Canvas canvas = obj.GetComponent<Canvas>();
            if(canvas) { canvas.overrideSorting = true; canvas.sortingOrder = 10; }
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
}
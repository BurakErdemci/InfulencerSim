using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;
    void Awake() { Instance = this; }

    [Header("Prefab Ayarları")]
    public GameObject playerPrefab;
    public GameObject[] goodItems;
    public GameObject[] badItems;

    [Header("UI Elementleri")]
    public Button startGameButton;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    [Header("Görsellik")]
    public GameObject gameBackground;

    [Header("UI Minigame Alanı")]
    public RectTransform gameArea;

    [Header("Oyun Ayarları")]
    public float gameDuration = 20f;
    public float spawnInterval = 0.45f;

    [Header("Spawn / Boyut")]
    public float objectScale = 1f;

    // ===================== LEVEL / PHASE =====================
    public enum DifficultyLevel { Level1, Level2, Level3 }

    [Header("Level Sistemi")]
    public DifficultyLevel currentLevel = DifficultyLevel.Level1;

    [Tooltip("Followers'a göre level otomatik seçilsin mi?")]
    public bool autoLevelByFollowers = true;

    [Tooltip("10k gibi değerler")]
    public long level2Followers = 10000;

    [Tooltip("1m gibi değerler")]
    public long level3Followers = 1000000;

    // ===================== SPEED RAMP (KAZIK) =====================
    [Header("KAZIK Speed Ramp")]
    [SerializeField] private float rampDuration = 30f;

    // Level1 başlangıç zaten hızlı
    [SerializeField] private float startSpeedMult = 2.2f;
    [SerializeField] private float endSpeedMult = 6.0f;

    [SerializeField] private AnimationCurve speedCurve =
        new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 0.65f),
            new Keyframe(1f, 1f)
        );

    // Level2 = "kolay faz" (yavaşlat)
    [Header("Level2 Easy (Slow Motion)")]
    [SerializeField] private float level2SlowMult = 0.45f;

    public float CurrentSpeedMultiplier { get; private set; } = 1f;

    // ===================== VACUUM (LEVEL3) =====================
    [Header("Level3 Auto Vacuum (NO INPUT)")]
    public float vacuumPullSpeed = 2800f;   // UI px/s, yüksek normal
    public float vacuumFallMult = 0.05f;    // vakumda düşüş neredeyse dursun
    public float vacuumCatchDistance = 55f; // collider'a bile gerek kalmadan topla

    private float vacuumEndTime = -1f; // Level3'te "sonsuz" yapacağız
    public bool IsVacuumActive => Time.time < vacuumEndTime;

    // ===================== INTERNAL =====================
    private bool isPlaying = false;
    private RectTransform currentPlayer;
    private int currentScore = 0;
    private float gameStartTime;

    public RectTransform PlayerRT => currentPlayer;

    void Update()
    {
        if (!isPlaying) return;

        // ---- SPEED RAMP ----
        float t = (Time.time - gameStartTime) / Mathf.Max(0.01f, rampDuration);
        t = Mathf.Clamp01(t);

        float eased = speedCurve.Evaluate(t);
        float baseMult = Mathf.Lerp(startSpeedMult, endSpeedMult, eased);

        float levelMult = 1f;
        if (currentLevel == DifficultyLevel.Level2) levelMult = level2SlowMult;

        CurrentSpeedMultiplier = baseMult * levelMult;
    }

    // ===================== PUBLIC API =====================
    public void SetupMinigame()
    {
        Cleanup();

        currentScore = 0;
        if (scoreText) scoreText.text = "Skor: 0";
        if (timerText) timerText.text = Mathf.CeilToInt(gameDuration).ToString();

        if (startGameButton) startGameButton.gameObject.SetActive(true);
        if (gameBackground) gameBackground.SetActive(false);

        if (startGameButton)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(StartMinigameLogic);
        }
    }

    // ===================== START =====================
    void StartMinigameLogic()
    {
        if (startGameButton) startGameButton.gameObject.SetActive(false);
        if (gameBackground) gameBackground.SetActive(true);

        isPlaying = true;
        gameStartTime = Time.time;

        // ---- AUTO LEVEL ----
        if (autoLevelByFollowers && GameManager.Instance != null)
        {
            long f = GameManager.Instance.followers;

            if (f >= level3Followers) currentLevel = DifficultyLevel.Level3;
            else if (f >= level2Followers) currentLevel = DifficultyLevel.Level2;
            else currentLevel = DifficultyLevel.Level1;
        }

        // ---- LEVEL3: AUTO VACUUM (NO INPUT) ----
        if (currentLevel == DifficultyLevel.Level3)
            vacuumEndTime = Time.time + 999999f; // pratikte sonsuz
        else
            vacuumEndTime = -1f;

        SpawnPlayer();

        StartCoroutine(SpawnRoutine());
        StartCoroutine(GameTimerRoutine());

        Debug.Log($"[LEVEL] followers={(GameManager.Instance != null ? GameManager.Instance.followers : -1)} level={currentLevel} vacuum={IsVacuumActive}");
    }

    void SpawnPlayer()
    {
        if (currentPlayer != null) Destroy(currentPlayer.gameObject);

        Transform parent = (gameArea != null) ? gameArea : transform;

        GameObject p = Instantiate(playerPrefab, parent);
        p.tag = "Player";

        currentPlayer = p.GetComponent<RectTransform>();

        if (currentPlayer != null && gameArea != null)
        {
            float y = -(gameArea.rect.height * 0.5f) + 60f;
            currentPlayer.anchoredPosition = new Vector2(0, y);
            currentPlayer.localScale = Vector3.one;
            currentPlayer.localRotation = Quaternion.identity;
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (isPlaying)
        {
            SpawnItem();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnItem()
    {
        if (gameArea == null) return;

        // Level3 power fantasy: çoğu good
        float goodChance = 0.6f;
        if (currentLevel == DifficultyLevel.Level2) goodChance = 0.8f;
        if (currentLevel == DifficultyLevel.Level3) goodChance = 0.9f;

        GameObject[] pool = (Random.value < goodChance) ? goodItems : badItems;
        if (pool == null || pool.Length == 0) return;

        GameObject prefab = pool[Random.Range(0, pool.Length)];
        GameObject obj = Instantiate(prefab, gameArea);

        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            float xRange = (gameArea.rect.width * 0.5f) - 60f;
            float y = (gameArea.rect.height * 0.5f) + 40f;

            rt.anchoredPosition3D = new Vector3(Random.Range(-xRange, xRange), y, 0f);
            rt.localScale = prefab.transform.localScale * objectScale;
            rt.localRotation = Quaternion.identity;
        }
    }

    public void AddScore(int val)
    {
        currentScore += val;
        if (scoreText) scoreText.text = "Skor: " + currentScore;
    }

    IEnumerator GameTimerRoutine()
    {
        float time = gameDuration;

        while (time > 0 && isPlaying)
        {
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

        if (MainController.Instance != null)
            MainController.Instance.CompleteStreamSession(currentScore);
    }

    void Cleanup()
    {
        StopAllCoroutines();

        if (currentPlayer != null)
        {
            Destroy(currentPlayer.gameObject);
            currentPlayer = null;
        }

        if (gameArea != null)
        {
            foreach (var f in gameArea.GetComponentsInChildren<FallingObject>(true))
                Destroy(f.gameObject);
        }
    }
}
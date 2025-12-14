using UnityEngine;

public class FallingObject : MonoBehaviour
{
    [Header("Base hız")]
    public float baseFallSpeed = 600f;

    [Header("Puanlar")]
    public int goodPoints = 50;
    public int badPoints = -150;

    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (rt == null || MinigameManager.Instance == null) return;

        var mg = MinigameManager.Instance;

        // Vakum ve Hız değerlerini MinigameManager'dan al
        float mult = mg.CurrentSpeedMultiplier;
        bool vacuum = mg.IsVacuumActive; // Phase 3 ise true döner

        // ---- FALL ----
        float fallMult = vacuum ? mg.vacuumFallMult : 1f;
        rt.anchoredPosition += Vector2.down * (baseFallSpeed * mult * fallMult) * Time.deltaTime;

        // ---- VACUUM PULL (GOD MODE) ----
        if (vacuum && mg.PlayerRT != null)
        {
            Vector2 playerPos = mg.PlayerRT.anchoredPosition;
            Vector2 dir = playerPos - rt.anchoredPosition;

            // Çekim
            rt.anchoredPosition += dir.normalized * mg.vacuumPullSpeed * Time.deltaTime;

            // Yakalama (Distance Check)
            if (dir.sqrMagnitude <= mg.vacuumCatchDistance * mg.vacuumCatchDistance)
            {
                if (CompareTag("Good")) mg.AddScore(goodPoints);
                else if (CompareTag("Bad")) mg.AddScore(badPoints);

                Destroy(gameObject);
                return;
            }
        }

        // ---- EKRAN DIŞINA ÇIKMA ----
        RectTransform parent = rt.parent as RectTransform;
        if (parent != null)
        {
            float bottom = -(parent.rect.height * 0.5f) - 140f;
            if (rt.anchoredPosition.y < bottom)
                Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (MinigameManager.Instance != null)
        {
            if (CompareTag("Good")) MinigameManager.Instance.AddScore(goodPoints);
            else if (CompareTag("Bad")) MinigameManager.Instance.AddScore(badPoints);
        }

        Destroy(gameObject);
    }
}
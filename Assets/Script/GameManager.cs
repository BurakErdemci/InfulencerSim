using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Oyun Verileri (İzlemek İçin)")]
    public long followers = 100;    // Takipçi
    public float morality = 100f;   // Ahlak (100 = İyi, 0 = Kötü)
    public int streamCount = 0;     // Kaçıncı yayındayız

    void Awake()
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
    
    public void UpdateStats(int followerGain, float moralityLoss)
    {
        followers += followerGain;
        morality -= moralityLoss;
        morality = Mathf.Clamp(morality, 0, 100);
        streamCount++;
    }
}
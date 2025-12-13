using UnityEngine;

public class FallingObject : MonoBehaviour
{
    public float fallSpeed = 10f; // Manager bunu değiştirecek
    public int goodPoints = 50;
    public int badPoints = -150; // CEZAYI ARTIRDIK (-100'den -150'ye)

    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Static Instance ile ulaşım (Daha performanslı)
            if (MinigameManager.Instance != null)
            {
                if (gameObject.CompareTag("Good"))
                    MinigameManager.Instance.AddScore(goodPoints);
                else if (gameObject.CompareTag("Bad"))
                    MinigameManager.Instance.AddScore(badPoints);
            }
            Destroy(gameObject);
        }
    }
}
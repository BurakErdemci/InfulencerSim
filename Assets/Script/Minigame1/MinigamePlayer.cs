using UnityEngine;

public class MinigamePlayer : MonoBehaviour
{
    public float speed = 15f;
    [Header("Sınırlar")]
    public float minX = -3f; // Sola ne kadar gitsin? (Senin bulduğun değer)
    public float maxX = 3f;  // Sağa ne kadar gitsin? (Bunu azaltacağız)

    public float smoothTime = 0.15f;

    private Vector3 currentVelocity;

    void Update()
    {
        // Mouse pozisyonunu al
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        Vector3 targetPos = new Vector3(mousePos.x, transform.position.y, -5f); 
        
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPos, 
            ref currentVelocity, 
            smoothTime
        );
    }
}
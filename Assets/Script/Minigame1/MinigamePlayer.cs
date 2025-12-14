using UnityEngine;

public class MinigamePlayer : MonoBehaviour
{
    public float smoothTime = 0.08f;

    RectTransform rt;
    Vector3 vel;
    Canvas canvas;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (rt == null) return;

        RectTransform parent = rt.parent as RectTransform;
        if (parent == null) return;

        // Canvas Overlay değilse worldCamera gerekir
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            Input.mousePosition,
            cam,
            out localPoint
        );

        // X'i parent genişliğine göre clamp et
        float halfW = parent.rect.width * 0.5f;
        float x = Mathf.Clamp(localPoint.x, -halfW + 60f, halfW - 60f);

        Vector3 target = new Vector3(x, rt.anchoredPosition.y, 0f);
        rt.anchoredPosition = Vector3.SmoothDamp(rt.anchoredPosition, target, ref vel, smoothTime);
    }
}
using UnityEngine;
using UnityEngine.EventSystems; // Bunu eklemeyi unutma!

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    // Mouse üzerine gelince çalışır (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayHover();
        }
    }

    // Tıklayınca çalışır (Click)
    public void OnPointerDown(PointerEventData eventData)
    {
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayClick();
        }
    }
}
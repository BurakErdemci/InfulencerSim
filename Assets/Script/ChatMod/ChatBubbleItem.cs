using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ChatBubbleItem : MonoBehaviour
{
    public TextMeshProUGUI contentText;
    public Image bubbleBg;
    public Button btn;
    
    [HideInInspector] public bool isHate;
    private ChatModGameManager manager;
    private float lifeTime;
    
    public void Setup(string text, bool hate, Color color, float time, ChatModGameManager mgr)
    {
        if(contentText)
        {
             contentText.text = text;
             contentText.enableAutoSizing = true;
             contentText.fontSizeMin = 10;
             contentText.fontSizeMax = 36;
        }
        if(bubbleBg) bubbleBg.color = color;
        
        isHate = hate;
        lifeTime = time;
        manager = mgr;
        
        if(btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }

        // Animate In
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        
        // Auto Destroy / Expire logic
        Invoke(nameof(Expire), lifeTime);
    }

    void OnClick()
    {
        transform.DOKill(); // Stop any animations
        if(manager != null) manager.OnCommentClicked(this);
    }

    void Expire()
    {
        if(manager != null) manager.OnCommentExpired(this);
        
        // Animate Out
        transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}

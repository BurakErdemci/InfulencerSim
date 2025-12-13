using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // Added for animations

public class TrendItem : MonoBehaviour
{
    [Header("UI References")]
    public Button button;
    public TextMeshProUGUI topicText;
    public Image backgroundImage;
    public Image iconImage; // NEW: Optional icon

    [Header("Data")]
    public string topicName;
    public int followerGain;
    public float moralityLoss;
    public bool isRisky;

    private TrendHuntManager manager;
    private bool isSelected = false;

    public void Setup(string topic, int gain, float morality, bool risky, TrendHuntManager mgr)
    {
        topicName = topic;
        followerGain = gain;
        moralityLoss = morality;
        isRisky = risky;
        manager = mgr;

        if (topicText != null) topicText.text = "#" + topicName;
        
        // Visual Setup
        UpdateVisuals();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
        
        // Entrance Animation
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    void UpdateVisuals()
    {
        if (backgroundImage == null) return;

        // Reset
        backgroundImage.color = isRisky ? new Color(1f, 0.8f, 0.8f) : Color.white;
        
        // If we want more complex styling, we can swap sprites here too
    }

    void OnClick()
    {
        if (manager == null) return;
        
        if (!isSelected)
        {
            if (manager.TrySelectTrend(this))
            {
                isSelected = true;
                if(backgroundImage != null) backgroundImage.color = new Color(0.5f, 1f, 0.5f); // Green Select
                
                // Pulse Animation
                transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
            }
        }
        else
        {
            manager.DeselectTrend(this);
            isSelected = false;
            UpdateVisuals(); // Revert color
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class StreamUIManager : MonoBehaviour
{
    [Header("Paneller")]
    public RectTransform roomPanel;  
    public RectTransform streamPanel; 
    
    [Header("Diğer Bağlantılar")]
    public Image faceCamImage;
    public Sprite[] faceStates; 
    public ChatManager chatManager;

   
    private float screenHeight = 1080f; 

    void Start()
    {
       
        streamPanel.gameObject.SetActive(false); 
        
        streamPanel.anchoredPosition = new Vector2(0, -screenHeight);
    }

    public void GoLive()
    {
       
        roomPanel.DOAnchorPosY(screenHeight, 0.8f).SetEase(Ease.InBack);


        streamPanel.gameObject.SetActive(true); 
        streamPanel.anchoredPosition = new Vector2(0, -screenHeight); 

    
        streamPanel.DOAnchorPosY(0, 0.8f).SetEase(Ease.OutBack).OnComplete(() => {
           
             chatManager.StartChat();
        });

        UpdateFacecam();
    }

    public void EndStream()
    {
        chatManager.StopChat();

   
        streamPanel.DOAnchorPosY(-screenHeight, 0.6f).SetEase(Ease.InBack).OnComplete(() => {
            streamPanel.gameObject.SetActive(false);
        });

     
        roomPanel.DOAnchorPosY(0, 0.8f).SetEase(Ease.OutBack);
    }

    void UpdateFacecam()
    {
        float m = GameManager.Instance.morality;
        if (m > 70) faceCamImage.sprite = faceStates[0];
        else if (m > 30) faceCamImage.sprite = faceStates[1];
        else faceCamImage.sprite = faceStates[2];
    }
}
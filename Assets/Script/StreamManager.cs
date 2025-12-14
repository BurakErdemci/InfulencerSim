using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // Video için gerekli kütüphane
using DG.Tweening;

public class StreamUIManager : MonoBehaviour
{
    [Header("Paneller")]
    public RectTransform roomPanel;  
    public RectTransform streamPanel; 
    
    [Header("Resim Bazlı Facecam (Eski Sistem)")]
    public Image faceCamImage;
    public Sprite[] faceStates; 

    [Header("Video Bazlı Facecam (YENİ)")]
    public RawImage faceCamRawImage; // Videonun göründüğü obje
    public VideoPlayer faceCamPlayer; // Videoyu oynatan bileşen
    public VideoClip[] videoStates; // 0: İyi, 1: Orta, 2: Kötü (Video halleri)

    [Header("Diğer")]
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
             // Video oynatmaya başla
             if(faceCamPlayer != null && faceCamPlayer.clip != null)
             {
                 faceCamPlayer.Play();
             }
        });

        UpdateFacecam();
    }

    public void EndStream()
    {
        chatManager.StopChat();

        // Videoyu durdur
        if(faceCamPlayer != null) faceCamPlayer.Stop();

        streamPanel.DOAnchorPosY(-screenHeight, 0.6f).SetEase(Ease.InBack).OnComplete(() => {
            streamPanel.gameObject.SetActive(false);
        });

        roomPanel.DOAnchorPosY(0, 0.8f).SetEase(Ease.OutBack);
    }

    void UpdateFacecam()
    {
        float m = GameManager.Instance.morality;
        int stateIndex = 0;

        if (m > 70) stateIndex = 0;
        else if (m > 30) stateIndex = 1;
        else stateIndex = 2;

        // --- VİDEO KONTROLÜ ---
        // Eğer VideoPlayer atanmışsa ve ilgili state için bir video varsa VİDEO oynat
        if (faceCamPlayer != null && faceCamRawImage != null && 
            videoStates.Length > stateIndex && videoStates[stateIndex] != null)
        {
            // 1. Resmi Gizle
            if(faceCamImage != null) faceCamImage.gameObject.SetActive(false);
            
            // 2. Videoyu Göster ve Ayarla
            faceCamRawImage.gameObject.SetActive(true);
            faceCamPlayer.clip = videoStates[stateIndex];
            faceCamPlayer.Play();
        }
        else
        {
            // --- RESİM KONTROLÜ (Fallback) ---
            // Video yoksa eski sistem çalışsın
            
            // 1. Videoyu Gizle
            if(faceCamRawImage != null) faceCamRawImage.gameObject.SetActive(false);
            
            // 2. Resmi Göster
            if(faceCamImage != null)
            {
                faceCamImage.gameObject.SetActive(true);
                faceCamImage.sprite = faceStates[stateIndex];
            }
        }
    }
}
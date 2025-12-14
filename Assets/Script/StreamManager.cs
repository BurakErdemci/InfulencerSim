using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; 
using DG.Tweening;

public class StreamUIManager : MonoBehaviour
{
    [Header("Paneller")]
    public RectTransform roomPanel;  
    public RectTransform streamPanel; 
    
    [Header("Resim Bazlı Facecam (Yedek)")]
    public Image faceCamImage;
    public Sprite[] faceStates; 

    [Header("Video Bazlı Facecam")]
    public RawImage faceCamRawImage; 
    public VideoPlayer faceCamPlayer; 
    public VideoClip[] videoStates; // 0: İyi, 1: Orta, 2: Kötü

    [Header("Diğer")]
    public ChatManager chatManager;

    private float screenHeight = 1080f; 

    void Start()
    {
        // Başlangıçta paneli ve facecam'i hazırla
        streamPanel.gameObject.SetActive(false); 
        streamPanel.anchoredPosition = new Vector2(0, -screenHeight);
        
        // Video player'ı hazırla ama oynatma
        if(faceCamPlayer != null) 
        {
            faceCamPlayer.playOnAwake = false;
            faceCamPlayer.Stop();
        }
    }

    public void GoLive()
    {
        roomPanel.DOAnchorPosY(screenHeight, 0.8f).SetEase(Ease.InBack);

        streamPanel.gameObject.SetActive(true); 
        streamPanel.anchoredPosition = new Vector2(0, -screenHeight); 

        streamPanel.DOAnchorPosY(0, 0.8f).SetEase(Ease.OutBack).OnComplete(() => {
             chatManager.StartChat();
             UpdateFacecam(); // Facecam'i animasyon bitince başlatmak daha güvenli
        });
    }

    public void EndStream()
    {
        chatManager.StopChat();
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

        // Duruma göre hangi videonun oynayacağını seç
        if (m > 70) stateIndex = 0;      
        else if (m > 30) stateIndex = 1; 
        else stateIndex = 2;             

        bool videoSuccess = false;

        if (faceCamPlayer != null && faceCamRawImage != null && 
            videoStates.Length > stateIndex && videoStates[stateIndex] != null)
        {
            try
            {
                // Hedef video klibi
                VideoClip targetClip = videoStates[stateIndex];

                
                if (faceCamPlayer.clip == targetClip && faceCamPlayer.isPlaying)
                {
                    // Video zaten doğru ve akıyor, elleme.
                    videoSuccess = true;
                }
                else
                {
                    // Video farklıysa veya durmuşsa değiştir
                    if(faceCamImage != null) faceCamImage.gameObject.SetActive(false);
                    
                    faceCamRawImage.gameObject.SetActive(true);
                    faceCamPlayer.gameObject.SetActive(true);

                    faceCamPlayer.isLooping = true; // Döngüde olduğundan emin ol
                    faceCamPlayer.clip = targetClip;
                    faceCamPlayer.Play();
                }
                
                videoSuccess = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Video hatası: " + e.Message);
                videoSuccess = false;
            }
        }

        // --- FALLBACK (Video başarısızsa resme dön) ---
        if (!videoSuccess)
        {
            if(faceCamRawImage != null) faceCamRawImage.gameObject.SetActive(false);
            if(faceCamPlayer != null) faceCamPlayer.Stop();
            
            if(faceCamImage != null)
            {
                faceCamImage.gameObject.SetActive(true);
                faceCamImage.sprite = faceStates[stateIndex];
            }
        }
    }
}
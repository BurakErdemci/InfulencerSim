using UnityEngine;
using UnityEngine.Video; // Video işlemleri için şart
using UnityEngine.SceneManagement; // Sahne değişimi için şart

public class IntroScene : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Video Player bileşenini buraya bağlayacağız
    public string mainMenuSceneName = "MainMenu"; // Ana menü sahnemizin tam adı

    void Start()
    {
        // Video bittiğinde ne olacağını sisteme abone ediyoruz
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void Update()
    {
        // Oyuncu sıkılıp geçmek isterse (Enter veya Tık)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            LoadMainMenu();
        }
    }

    // Video bitince otomatik çalışacak fonksiyon
    void OnVideoFinished(VideoPlayer vp)
    {
        LoadMainMenu();
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
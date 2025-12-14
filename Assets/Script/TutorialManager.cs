using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public void NextScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void BeforeScene()
    {
        SceneManager.LoadScene("mainMenu");
    }
}

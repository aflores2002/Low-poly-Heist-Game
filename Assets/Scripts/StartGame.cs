using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public string sceneToLoad = "DemoScene_Nick"; // Change to your main scene name

    public void LoadGame()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}

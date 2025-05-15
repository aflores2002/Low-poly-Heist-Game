using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CaughtScreenUI : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI stolenText;

    void Start()
    {
        int stolenAmount = PlayerPrefs.GetInt("StolenAmount", 0);
        resultText.text = "YOU'VE BEEN CAUGHT";
        stolenText.text = $"Jewelry stolen: ${stolenAmount}";
    }

    public void Retry()
    {
        SceneManager.LoadScene("DemoScene_Nick"); // Replace with your actual game scene name
    }

    public void Quit()
    {
        Application.Quit();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class ExtractZone : MonoBehaviour
{
    private bool hasExtracted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasExtracted) return;

        if (other.CompareTag("Player"))
        {
            hasExtracted = true;

            // Store the final money for use in the next scene
            PlayerMoney money = other.GetComponent<PlayerMoney>();
            if (money != null)
            {
                PlayerPrefs.SetInt("FinalMoney", money.currentMoney);
            }

            // Play car SFX
            AudioManager.Instance?.PlayCarDriveOff();

            // Load the end screen scene
            SceneManager.LoadScene("EndScene");
        }
    }
}

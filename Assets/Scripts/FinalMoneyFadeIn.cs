using UnityEngine;
using TMPro;
using System.Collections;

public class FinalMoneyFadeIn : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public CanvasGroup canvasGroup;
    public float fadeDuration = 2f;
    public float fadeDelay = 1f;

    void Start()
    {
        canvasGroup.alpha = 0f; // Start fully transparent
        int finalMoney = PlayerPrefs.GetInt("FinalMoney", 0);
        moneyText.text = $"You Escaped With ${finalMoney:N0}";

        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(fadeDelay);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f; // Ensure it's fully visible at the end
    }
}

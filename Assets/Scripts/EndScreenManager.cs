using UnityEngine;
using TMPro;

public class EndScreenManager : MonoBehaviour
{
    public TextMeshProUGUI finalMoneyText;

    void Start()
    {
        int finalMoney = PlayerPrefs.GetInt("FinalMoney", 0);
        finalMoneyText.text = $"You Escaped With ${finalMoney:N0}";
    }
}

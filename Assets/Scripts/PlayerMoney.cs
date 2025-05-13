using UnityEngine;
using TMPro;

public class PlayerMoney : MonoBehaviour
{
    public int currentMoney = 0;
    public TextMeshProUGUI moneyText;

    private void Start()
    {
        UpdateMoneyUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        moneyText.text = $"${currentMoney:N0}";
    }
}

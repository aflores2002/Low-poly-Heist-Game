using UnityEngine;
using TMPro;

public class PlayerMoney : MonoBehaviour
{
    public int currentMoney = 0;
    public TextMeshProUGUI moneyText;

    [Header("Money Pop-Up")]
    public GameObject moneyPopupPrefab;
    public Transform popupParent;

    private void Start()
    {
        UpdateMoneyUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();

        // Show floating text
        if (moneyPopupPrefab != null && popupParent != null)
        {
            GameObject popup = Instantiate(moneyPopupPrefab, popupParent);
            popup.transform.localPosition = Vector3.zero; // center in parent
            popup.GetComponent<MoneyPopup>().SetAmount(amount);
        }
        // Play cash register SFX
        AudioManager.Instance?.PlayCashRegister();
    }

    private void UpdateMoneyUI()
    {
        moneyText.text = $"${currentMoney:N0}";
    }
}

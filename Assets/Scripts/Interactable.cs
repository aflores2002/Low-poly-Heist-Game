using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string promptMessage = "Steal";

    public virtual void Interact(GameObject interactor)
    {
        CopPatrol cop = GameObject.FindWithTag("Cop")?.GetComponent<CopPatrol>();
        if (cop != null)
        {
            cop.AlertCop(); // Optional if you want suspicion only when spotted
        }

        JewelrySpot spot = GetComponentInParent<JewelrySpot>();
        if (spot != null)
        {
            spot.MarkAsStolen(); // Tell the anchor the item is gone
        }

        // 3. Add money to player (Randomly chooses from $1000, $2000, or $3000)
        PlayerMoney moneySystem = interactor.GetComponent<PlayerMoney>();
        if (moneySystem != null)
        {
            int[] values = { 1000, 2000, 3000 };
            int reward = values[Random.Range(0, values.Length)];
            moneySystem.AddMoney(reward);
        }

        Destroy(gameObject); // Simulate theft
    }
}


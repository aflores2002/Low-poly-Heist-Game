using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string promptMessage = "Steal";

    public virtual void Interact()
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

        Destroy(gameObject); // Simulate theft
    }
}


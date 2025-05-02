using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string promptMessage = "Steal";

    public virtual void Interact()
    {
        // Notify the cop
        CopPatrol cop = GameObject.FindWithTag("Cop")?.GetComponent<CopPatrol>();
        if (cop != null)
        {
            cop.AlertCop();
        }

        // Simulate stealing
        Destroy(gameObject);
    }
}

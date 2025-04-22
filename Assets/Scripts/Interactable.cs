using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string promptMessage = "Steal";

    public virtual void Interact()
    {
        Destroy(gameObject); // This simulates stealing the item
    }
}

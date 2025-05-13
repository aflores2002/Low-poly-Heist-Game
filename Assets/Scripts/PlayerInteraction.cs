using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 3f;
    public Transform cameraTransform;
    public GameObject interactionPromptUI;
    public TextMeshProUGUI interactionText;

    private Interactable currentInteractable;

    void Update()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                interactionPromptUI.SetActive(true);
                interactionText.text = $"[E] {interactable.promptMessage}";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    currentInteractable.Interact(gameObject); // Pass the player object
                    interactionPromptUI.SetActive(false);
                }

                return;
            }
        }

        currentInteractable = null;
        interactionPromptUI.SetActive(false);
    }
}

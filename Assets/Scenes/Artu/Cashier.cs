using UnityEngine;
using UnityEngine.AI;
public class CashierAI : MonoBehaviour {
    public AudioClip panicClip;
    private AudioSource audioSource;
    private Animator animator;
    private bool hasPanicked = false;

    void Start() {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    public void TriggerPanic() {
        if (hasPanicked) return;

        hasPanicked = true;
        audioSource.PlayOneShot(panicClip);
        animator.SetTrigger("panic"); // make sure your Animator has a "Panic" trigger
        // Optional: Disable movement or other scripts
    }
}

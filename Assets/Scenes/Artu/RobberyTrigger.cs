using UnityEngine;
public class RobberyTrigger : MonoBehaviour {
    private bool robberyStarted = false;
    private bool Panic = false;

    void OnTriggerEnter(Collider other) {
        if (robberyStarted) return;

        if (other.CompareTag("Player")) {
            robberyStarted = true;
            Panic = true;
            TriggerPanicBehaviors();
        }
    }

    void TriggerPanicBehaviors() {
        // Panic the cashier
        var cashier = FindObjectOfType<CashierAI>();
        if (cashier != null) {
            cashier.TriggerPanic();
        }

        // Flee civilians
        CivilianFleeAI[] civilians = FindObjectsOfType<CivilianFleeAI>();
        foreach (var civilian in civilians) {
            civilian.TriggerFlee();
        }
    }
}

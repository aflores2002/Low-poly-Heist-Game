using UnityEngine.AI;
using UnityEngine;

public class CivilianFleeAI : MonoBehaviour {
    public Transform fleeTarget; // assign a point far from the store
    private NavMeshAgent agent;
    private bool isFleeing = false;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
    }

    public void TriggerFlee() {
        if (isFleeing) return;

        isFleeing = true;
        agent.SetDestination(fleeTarget.position);
        // Optional: Play flee animation or sound
    }
}


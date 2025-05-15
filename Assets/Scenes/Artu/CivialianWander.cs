using UnityEngine;
using UnityEngine.AI;

public class CivilianWander : MonoBehaviour {
    public Transform[] waypoints;
    private NavMeshAgent agent;
    private int currentTarget;
    private bool isFleeing = false;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        ChooseNextDestination();
    }

    void Update() {
        if (isFleeing) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f) {
            ChooseNextDestination();
        }
    }

    void ChooseNextDestination() {
    if (waypoints.Length == 0) return;

    currentTarget = (currentTarget + 1) % waypoints.Length;
    agent.SetDestination(waypoints[currentTarget].position);
}


    public void TriggerFlee(Transform fleeTarget) {
        isFleeing = true;
        agent.SetDestination(fleeTarget.position);
    }
}


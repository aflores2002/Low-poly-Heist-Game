using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CopPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float waitTimeAtWaypoint = 2f; // Time to idle
    private int currentPoint = 0;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        GoToNextPatrol();
    }

    void Update()
    {
        if (isWaiting) return;

        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        // Stop movement and play idle
        agent.isStopped = true;
        animator.SetFloat("Speed", 0f);

        yield return new WaitForSeconds(waitTimeAtWaypoint);

        currentPoint = (currentPoint + 1) % patrolPoints.Length;
        GoToNextPatrol();
        isWaiting = false;
    }

    void GoToNextPatrol()
    {
        if (patrolPoints.Length == 0) return;

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPoint].position);
    }
}



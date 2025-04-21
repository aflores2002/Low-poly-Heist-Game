using UnityEngine;
using UnityEngine.AI;

public enum CopState 
{
    Patrol,
    Investigate,
    Chase,
    ReturnToPost
}

public class CopStateMachine : MonoBehaviour
{
   
    public Transform[] patrolPoints;
    public float detectionRange = 10f;
    public float investigateTime = 3f;
    public Transform player;

    private int currentPoint = 0;
    private NavMeshAgent agent;
    private Animator animator;
    private CopState currentState = CopState.Patrol;
    private Vector3 lastKnownPosition;
    private float investigateTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        GoToNextPatrol();
    }

    void Update()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        switch (currentState)
        {
            case CopState.Patrol:
                Patrol();
                break;
            case CopState.Investigate:
                Investigate();
                break;
            case CopState.Chase:
                Chase();
                break;
            case CopState.ReturnToPost:
                ReturnToPost();
                break;
        }

        CheckForPlayer();
    }

    void Patrol()
    {
        if (agent.remainingDistance < 0.5f)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
            GoToNextPatrol();
        }
    }

    void Investigate()
    {
        investigateTimer += Time.deltaTime;

        if (investigateTimer >= investigateTime)
        {
            investigateTimer = 0f;
            currentState = CopState.ReturnToPost;
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }

    void Chase()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }

        if (Vector3.Distance(transform.position, player.position) > detectionRange * 1.5f)
        {
            currentState = CopState.ReturnToPost;
        }
    }

    void ReturnToPost()
    {
        if (agent.remainingDistance < 0.5f)
        {
            currentState = CopState.Patrol;
            GoToNextPatrol();
        }
    }

    void CheckForPlayer()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < detectionRange)
        {
            currentState = CopState.Chase;
        }
    }

    void GoToNextPatrol()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPoint].position);
    }

    public void TriggerInvestigation(Vector3 location)
    {
        currentState = CopState.Investigate;
        lastKnownPosition = location;
        agent.SetDestination(lastKnownPosition);
    }
}

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CopPatrol : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waitTimeAtWaypoint = 2f;
    private int currentPoint = 0;
    private bool isWaiting = false;

    [Header("Detection")]
    public Transform player;
    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 120f;
    public LayerMask playerMask;
    public LayerMask obstructionMask;

    [Header("Alert Settings")]
    public float suspicionThreshold = 100f;
    public float suspicionPerTheft = 25f;
    public float chaseCooldown = 5f;

    [Header("Movement Speeds")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("UI")]
    public Slider suspicionSlider;
    public TextMeshProUGUI suspicionText;

    [Header("Escalation Settings")]
    public float increasedViewRadius = 15f;
    public float increasedViewAngle = 160f;

    private float suspicionLevel = 0f;
    private bool isChasing = false;
    private bool isAlerted = false;
    private float lastSeenTime = 0f;
    private bool hasEscalated = false;

    private NavMeshAgent agent;
    private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int ChasingHash = Animator.StringToHash("Chasing");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (suspicionSlider != null)
        {
            suspicionSlider.maxValue = suspicionThreshold;
            suspicionSlider.value = 0f;
        }

        GoToNextPatrol();
    }

    void Update()
    {
        UpdateUI();

        if (isChasing)
        {
            agent.speed = chaseSpeed;
            animator.SetBool(ChasingHash, true);

            if (CanSeePlayer())
            {
                lastSeenTime = Time.time;
                agent.SetDestination(player.position);
            }
            else if (Time.time - lastSeenTime > chaseCooldown)
            {
                isChasing = false;
                animator.SetBool(ChasingHash, false);
                GoToNextPatrol();
            }

            animator.SetFloat(SpeedHash, agent.velocity.magnitude);
            return;
        }

        if (isAlerted && CanSeePlayer())
        {
            isChasing = true;
            lastSeenTime = Time.time;
            return;
        }

        if (isWaiting) return;

        agent.speed = patrolSpeed;
        animator.SetBool(ChasingHash, false);
        animator.SetFloat(SpeedHash, agent.velocity.magnitude);

        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        agent.isStopped = true;
        animator.SetFloat(SpeedHash, 0f);

        yield return new WaitForSeconds(waitTimeAtWaypoint);

        currentPoint = (currentPoint + 1) % patrolPoints.Length;
        GoToNextPatrol();
        isWaiting = false;
    }

    void GoToNextPatrol()
    {
        if (patrolPoints.Length == 0) return;

        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.SetDestination(patrolPoints[currentPoint].position);
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < viewRadius)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle < viewAngle / 2f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstructionMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void AlertCop()
    {
        isAlerted = true;
        suspicionLevel += suspicionPerTheft;
        suspicionLevel = Mathf.Clamp(suspicionLevel, 0, suspicionThreshold);

        if (suspicionSlider != null)
            suspicionSlider.value = suspicionLevel;

        Debug.Log($"Cop alerted! Suspicion level: {suspicionLevel}");

        if (suspicionLevel >= suspicionThreshold && !hasEscalated)
        {
            EscalateCop();
        }
    }

    void EscalateCop()
    {
        hasEscalated = true;
        viewRadius = increasedViewRadius;
        viewAngle = increasedViewAngle;

        Debug.Log("Cop has escalated! Detection radius and view angle increased.");
        // Optionally increase speeds or add new behavior here
    }

    void UpdateUI()
    {
        if (suspicionSlider != null)
        {
            suspicionSlider.value = Mathf.Lerp(suspicionSlider.value, suspicionLevel, Time.deltaTime * 5f);
        }

        if (suspicionText != null)
        {
            suspicionText.text = $"{Mathf.RoundToInt(suspicionLevel)} / {suspicionThreshold}";
        }
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftView = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightView = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftView * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightView * viewRadius);
    }
}



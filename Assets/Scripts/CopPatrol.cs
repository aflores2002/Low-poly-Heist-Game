using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class CopPatrol : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waitTimeAtWaypoint = 2f;
    private int currentPoint = 0;
    private bool isWaiting = false;

    [Header("Detection")]
    public Transform player;
    public LayerMask playerMask;
    public LayerMask obstructionMask;

    [Header("Suspicion")]
    public float suspicionThreshold = 100f;
    public float suspicionPerTheft = 25f;
    public float chaseCooldown = 5f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("UI")]
    public Slider suspicionSlider;
    public TextMeshProUGUI suspicionText;

    [Header("FOV Scaling")]
    public float minViewRadius = 10f;
    public float maxViewRadius = 20f;
    public float minViewAngle = 100f;
    public float maxViewAngle = 160f;

    [Header("Investigation")]
    public float objectDetectionRadius = 10f;
    public float investigateWaitTime = 2f;
    public LayerMask stolenObjectMask;

    private float suspicionLevel = 0f;
    private bool hasEscalated = false;
    private bool isInvestigating = false;
    private float lastSeenTime = 0f;
    private Vector3 lastKnownPlayerPosition;
    private bool isSearchingForPlayer = false;

    private float viewRadius;
    private float viewAngle;

    private NavMeshAgent agent;
    private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int ChasingHash = Animator.StringToHash("Chasing");

    private enum CopState { Patrol, Investigating, Chasing }
    private CopState currentState = CopState.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (suspicionSlider != null)
        {
            suspicionSlider.maxValue = suspicionThreshold;
            suspicionSlider.value = 0f;
        }

        viewRadius = minViewRadius;
        viewAngle = minViewAngle;

        GoToNextPatrol();
    }

    void Update()
    {
        UpdateSuspicionView();
        UpdateUI();

        switch (currentState)
        {
            case CopState.Chasing:
                HandleChase();
                break;

            case CopState.Investigating:
                break;

            case CopState.Patrol:
                if (ShouldChasePlayer())
                {
                    StartChase();
                }
                else if (ScanForStolenJewelry())
                {
                }
                else
                {
                    PatrolLogic();
                }
                break;
        }
    }

    void UpdateSuspicionView()
    {
        float t = suspicionLevel / suspicionThreshold;
        viewRadius = Mathf.Lerp(minViewRadius, maxViewRadius, t);
        viewAngle = Mathf.Lerp(minViewAngle, maxViewAngle, t);
    }

    bool ShouldChasePlayer()
    {
        return suspicionLevel > 0 && CanSeePlayer();
    }

    void StartChase()
    {
        currentState = CopState.Chasing;
        lastSeenTime = Time.time;
        animator.SetBool(ChasingHash, true);
        agent.speed = chaseSpeed;
        lastKnownPlayerPosition = player.position;
    }

    void HandleChase()
    {
        if (CanSeePlayer())
        {
            lastSeenTime = Time.time;
            lastKnownPlayerPosition = player.position;
            agent.SetDestination(player.position);
        }
        else if (!isSearchingForPlayer)
        {
            isSearchingForPlayer = true;
            StartCoroutine(InvestigateLastSeenPosition());
        }

        animator.SetFloat(SpeedHash, agent.velocity.magnitude);
    }

    IEnumerator InvestigateLastSeenPosition()
    {
        agent.SetDestination(lastKnownPlayerPosition);
        agent.isStopped = false;

        while (Vector3.Distance(transform.position, lastKnownPlayerPosition) > 1.5f)
        {
            if (CanSeePlayer())
            {
                isSearchingForPlayer = false;
                StartChase();
                yield break;
            }
            yield return null;
        }

        agent.isStopped = true;
        animator.SetFloat(SpeedHash, 0f);
        animator.SetBool("IsLookingAround", true); // Start looking animation


        float searchTime = 3f;
        float timer = 0f;

        while (timer < searchTime)
        {
            if (CanSeePlayer())
            {
                isSearchingForPlayer = false;
                StartChase();
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        animator.SetBool("IsLookingAround", false); // Stop looking
        isSearchingForPlayer = false;
        currentState = CopState.Patrol;
        animator.SetBool(ChasingHash, false);
        GoToNextPatrol();
    }

    bool ScanForStolenJewelry()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, objectDetectionRadius, stolenObjectMask);

        foreach (var hit in hits)
        {
            JewelrySpot spot = hit.GetComponent<JewelrySpot>();
            if (spot != null && spot.isEmpty && !spot.hasBeenInvestigated && CanSeePoint(spot.transform.position))
            {
                spot.hasBeenInvestigated = true;
                StartCoroutine(InvestigateSpot(spot.transform.position));
                return true;
            }
        }

        return false;
    }

    IEnumerator InvestigateSpot(Vector3 targetPosition)
    {
        currentState = CopState.Investigating;
        isInvestigating = true;


        Vector3 destination = targetPosition;
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            destination = hit.position;
        }
        else
        {
            isInvestigating = false;
            currentState = CopState.Patrol;
            GoToNextPatrol();
            yield break;
        }

        agent.SetDestination(destination);
        agent.isStopped = false;

        while (Vector3.Distance(transform.position, destination) > 1.5f)
        {
            if (ShouldChasePlayer())
            {
                isInvestigating = false;
                StartChase();
                yield break;
            }
            yield return null;
        }

        agent.isStopped = true;
        animator.SetBool("IsLookingAround", true);

        float timer = 0f;
        while (timer < investigateWaitTime)
        {
            if (ShouldChasePlayer())
            {
                isInvestigating = false;
                StartChase();
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        AlertCop();
        animator.SetBool("IsLookingAround", false);

        isInvestigating = false;
        agent.isStopped = false;
 
        currentState = CopState.Patrol;
        GoToNextPatrol();
    }

    void PatrolLogic()
    {
        if (isWaiting || isInvestigating) return;

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
        agent.SetDestination(patrolPoints[currentPoint].position);
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dir = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);
        float angle = Vector3.Angle(transform.forward, dir);

        if (dist < viewRadius && angle < viewAngle / 2f)
        {
            if (!Physics.Raycast(transform.position + Vector3.up, dir, dist, obstructionMask))
                return true;
        }

        return false;
    }

    bool CanSeePoint(Vector3 worldPos)
    {
        Vector3 dir = (worldPos - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, worldPos);
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle < viewAngle / 2f)
        {
            if (!Physics.Raycast(transform.position + Vector3.up, dir, dist, obstructionMask))
                return true;
        }

        return false;
    }

    public void AlertCop()
    {
        suspicionLevel += suspicionPerTheft;
        suspicionLevel = Mathf.Clamp(suspicionLevel, 0, suspicionThreshold);

        if (suspicionSlider != null)
            suspicionSlider.value = suspicionLevel;

        if (suspicionLevel >= suspicionThreshold && !hasEscalated)
        {
            hasEscalated = true;
        }
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

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + left * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * viewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lastKnownPlayerPosition, 0.3f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (currentState == CopState.Chasing && other.CompareTag("Player"))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("CaughtScene"); 
        }
    }
}


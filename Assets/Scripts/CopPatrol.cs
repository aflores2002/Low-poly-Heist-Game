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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip walkClip;
    public AudioClip runClip;

    [Header("Voice Clips")]
    public AudioClip gruntClip;
    public AudioClip confusedClip;



    private float suspicionLevel = 0f;
    private bool hasEscalated = false;
    private bool isInvestigating = false;
    private float lastSeenTime = 0f;
    private Vector3 lastKnownPlayerPosition;
    private bool isSearchingForPlayer = false;
    private bool isHit = false;

    private float viewRadius;
    private float viewAngle;

    private NavMeshAgent agent;
    private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int ChasingHash = Animator.StringToHash("Chasing");
    private static readonly int LookingHash = Animator.StringToHash("IsLookingAround");

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

        currentState = CopState.Patrol;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(navHit.position);
        }

        agent.enabled = true;
        agent.speed = patrolSpeed;
        agent.isStopped = false;

        animator.SetBool("CanMove", true); // allow movement from the beginning

        GoToNextPatrol();
    }

    void Update()
    {
        if (isHit)
        {
            StopAgent();
            return;
        }

        // Animator-controlled movement
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool canMove = animator.GetBool("CanMove");
        bool isBlocked = stateInfo.IsName("dying") || stateInfo.IsName("getting up");

        if (canMove && !isBlocked && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        animator.SetFloat(SpeedHash, agent.velocity.magnitude);

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
        UpdateFootstepAudio();

    }

    void UpdateFootstepAudio()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log("Current animation state: " + stateInfo.fullPathHash);

        if (stateInfo.IsName("Walking"))
        {
            Debug.Log("Playing walk audio");
            PlayFootstepClip(walkClip);
        }
        else if (stateInfo.IsName("Running"))
        {
            Debug.Log("Playing run audio");
            PlayFootstepClip(runClip);
        }
        else
        {
            if (audioSource.isPlaying)
            {
                Debug.Log("Stopping audio");
                audioSource.Stop();
            }
        }
    }


    void PlayFootstepClip(AudioClip clip)
    {
        if (audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }




    void StopAgent()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    void ResumeAgent()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
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
        // If max suspicion, chase no matter what
        if (suspicionLevel >= suspicionThreshold)
            return true;

        // Otherwise, only chase if suspicion exists AND player is visible
        return suspicionLevel > 0 && CanSeePlayer();
    }


    void StartChase()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        currentState = CopState.Chasing;
        lastSeenTime = Time.time;
        animator.SetBool(ChasingHash, true);
        animator.SetBool(LookingHash, false);
        agent.speed = chaseSpeed;
        agent.isStopped = false;
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
    }

    IEnumerator InvestigateLastSeenPosition()
    {
        agent.SetDestination(lastKnownPlayerPosition);
        ResumeAgent();

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

        StopAgent();
        animator.SetFloat(SpeedHash, 0f);
        animator.SetBool(LookingHash, true);

        float searchTime = 3f;
        float timer = 0f;

        while (timer < searchTime)
        {
            if (CanSeePlayer())
            {
                isSearchingForPlayer = false;
                animator.SetBool(LookingHash, false);
                StartChase();
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        animator.SetBool(LookingHash, false);
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
                PlayVoice(confusedClip); 

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

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            isInvestigating = false;
            currentState = CopState.Patrol;
            GoToNextPatrol();
            yield break;
        }

        ResumeAgent();

        while (Vector3.Distance(transform.position, hit.position) > 1.5f)
        {
            if (ShouldChasePlayer())
            {
                isInvestigating = false;
                StartChase();
                yield break;
            }
            yield return null;
        }

        StopAgent();
        animator.SetBool(LookingHash, true);

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
        animator.SetBool(LookingHash, false);

        isInvestigating = false;
        ResumeAgent();
        currentState = CopState.Patrol;
        GoToNextPatrol();
    }

    void PatrolLogic()
    {
        if (isWaiting || isInvestigating) return;

        agent.speed = patrolSpeed;
        animator.SetBool(ChasingHash, false);

        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        StopAgent();
        animator.SetFloat(SpeedHash, 0f);

        yield return new WaitForSeconds(waitTimeAtWaypoint);

        currentPoint = (currentPoint + 1) % patrolPoints.Length;
        GoToNextPatrol();
        isWaiting = false;
    }

    void GoToNextPatrol()
    {
        if (patrolPoints.Length == 0 || patrolPoints[currentPoint] == null) return;

        ResumeAgent();
        agent.speed = patrolSpeed;
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

    void OnTriggerEnter(Collider other)
    {
        if (isHit) return;

        if (other.CompareTag("Bullet"))
        {
            StartCoroutine(HandleKnockdown());
        }

        if (currentState == CopState.Chasing && other.CompareTag("Player"))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("CaughtScene");

            PlayerMoney playerMoney = FindObjectOfType<PlayerMoney>();
            if (playerMoney != null)
            {
                PlayerPrefs.SetInt("StolenAmount", playerMoney.currentMoney);
                PlayerPrefs.Save();
            }
        }
    }

    IEnumerator HandleKnockdown()
    {
        isHit = true;
        currentState = CopState.Patrol;


        StopAgent();
        agent.enabled = false;

        animator.applyRootMotion = false; // Disable root motion for knockdown


        animator.SetBool("CanMove", false);
        animator.SetTrigger("IsHit");

        PlayVoice(gruntClip);


        suspicionLevel = suspicionThreshold;
        if (suspicionSlider != null)
            suspicionSlider.value = suspicionLevel;

        yield return new WaitForSeconds(5f); // Dying animation

        animator.SetTrigger("IsGettingUp");

        yield return new WaitForSeconds(1f); // Getting up animation

        agent.enabled = true;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        animator.SetBool("CanMove", true);
        isHit = false;

        
        StartChase();
    }

    void PlayVoice(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }



}

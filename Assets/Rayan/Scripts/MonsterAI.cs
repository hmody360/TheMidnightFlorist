using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class MonsterAI : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static MonsterAI Instance { get; private set; }

    // ==================== ENUMS ====================
    public enum MonsterState
    {
        Patrol,      // Walking between random patrol points
        Alert,       // Scream then rush to flower position
        Investigate, // Arrive at flower, scream, notice flower gone
        Trace,       // Follow player's trail points at walk speed
        Search,      // Look around at end of trail
        Chase,       // Run after visible player
        Guard,       // Patrol near a flower location
        Attack       // Attack windup then jumpscare
    }

    // Alert has two phases
    private enum AlertPhase
    {
        Screaming,
        Rushing
    }

    // ==================== INSPECTOR SETTINGS ====================
    [Header("=== PATROL SETTINGS ===")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float waitTimeAtPoint = 2f;

    [Header("=== CHASE SETTINGS ===")]
    public float chaseSpeed = 5f;
    public float reactionDelay = 0.5f;

    [Header("=== ALERT/RUSH SETTINGS ===")]
    [Tooltip("Speed when rushing to flower (2x patrol speed by default)")]
    public float rushSpeed = 4f;
    [Tooltip("Time monster screams before rushing to flower")]
    public float alertScreamDuration = 1.5f;

    [Header("=== INVESTIGATE SETTINGS ===")]
    [Tooltip("Time monster screams/reacts when finding flower gone")]
    public float investigateScreamDuration = 2f;

    [Header("=== TRACE SETTINGS ===")]
    [Tooltip("How close player must be to patrol point to record it as trail")]
    public float trailRecordDistance = 4f;
    [Tooltip("Maximum speed during unlimited trace (monster speeds up over time)")]
    public float maxTraceSpeed = 12f;
    [Tooltip("Time in seconds to reach max trace speed (for unlimited trace)")]
    public float traceSpeedUpTime = 60f;

    [Header("=== SEARCH SETTINGS ===")]
    [Tooltip("How long monster searches at end of trail")]
    public float searchDuration = 4f;

    [Header("=== GUARD SETTINGS ===")]
    [Tooltip("How many patrol points to use for guard area")]
    public int guardPointCount = 4;

    [Header("=== VISION SETTINGS ===")]
    public float visionRange = 15f;
    public float visionAngle = 90f;
    public float eyeHeight = 1.5f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("=== CHASE LOSS SETTINGS ===")]
    public float losePlayerTime = 3f;
    [Tooltip("After losing player in chase, trace this many points")]
    public int traceAfterLoseChase = 3;

    [Header("=== ATTACK SETTINGS ===")]
    public float attackRange = 1.5f;
    [Tooltip("Duration of attack animation (player can escape during this)")]
    public float attackAnimationDuration = 0.867f;
    [Tooltip("Range to check if player was hit after attack animation")]
    public float attackHitRange = 2f;

    [Header("=== REFERENCES ===")]
    public Transform player;

    [Header("=== DEBUG ===")]
    public bool showDebugLogs = true;
    public bool showGizmos = true;

    // ==================== PRIVATE VARIABLES ====================
    // Components
    private NavMeshAgent agent;

    // State Machine
    private MonsterState currentState = MonsterState.Patrol;
    private MonsterState stateBeforeChase = MonsterState.Patrol; // Remember what state to return to
    private bool wasInGuardMode = false; // Track if we were guarding before chase

    // Patrol
    private int currentPatrolIndex;
    private float waitTimer;
    private bool isWaitingAtPoint = false;

    // Vision & Chase
    private bool canSeePlayer = false;
    private Vector3 lastKnownPlayerPosition;
    private float losePlayerTimer;
    private float reactionTimer;
    private bool isReacting = false;

    // Alert & Investigate
    private Vector3 targetFlowerPosition;
    private float alertTimer;
    private float investigateTimer;
    private bool isScreaming = false;
    private AlertPhase alertPhase = AlertPhase.Screaming; // FIX: Track alert phase

    // Trail System
    private List<Transform> playerTrail = new List<Transform>();
    private int currentTrailIndex = 0;
    private int trailPointsToFollow = -1; // -1 means follow all, positive number = limit
    private int trailPointsFollowed = 0;
    private float traceTimer = 0f; // Timer for speed increase during unlimited trace
    private HashSet<Transform> recordedTrailPoints = new HashSet<Transform>();
    private bool isRecordingTrail = false;
    private bool isTraceFromChaseLoss = false; // Flag to handle special case when losing player

    // Search
    private float searchTimer;

    // Guard
    private List<Transform> guardPoints = new List<Transform>();
    private int currentGuardIndex = 0;
    private Vector3 guardFlowerPosition;

    // Attack
    private bool isAttacking = false;
    private float attackTimer;

    // Night tracking
    private int currentNight = 1;
    private int flowersCollectedThisNight = 0;
    private int totalFlowersThisNight = 2;
    private bool isLastFlower = false;

    // ==================== UNITY METHODS ====================
    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("MonsterAI: Multiple instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("MonsterAI: NavMeshAgent component required!");
            return;
        }

        // Auto-find player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("MonsterAI: No player found! Tag your player with 'Player' tag.");
            }
        }

        // Validate patrol points
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("MonsterAI: No patrol points assigned!");
        }

        // Initialize rush speed if not set
        if (rushSpeed == 0)
        {
            rushSpeed = patrolSpeed * 2f;
        }

        // Start patrolling
        SetState(MonsterState.Patrol);
    }

    void Update()
    {
        if (agent == null) return;

        // Always check vision (except during screaming/windup/alert rushing)
        // FIX: Don't check vision during Alert at all - monster should go to flower first
        if (!isScreaming && !isAttacking && currentState != MonsterState.Alert)
        {
            CheckVision();
        }

        // Record player trail if enabled
        if (isRecordingTrail)
        {
            RecordPlayerTrail();
        }

        // Run current state logic
        switch (currentState)
        {
            case MonsterState.Patrol:
                UpdatePatrol();
                break;
            case MonsterState.Alert:
                UpdateAlert();
                break;
            case MonsterState.Investigate:
                UpdateInvestigate();
                break;
            case MonsterState.Trace:
                UpdateTrace();
                break;
            case MonsterState.Search:
                UpdateSearch();
                break;
            case MonsterState.Chase:
                UpdateChase();
                break;
            case MonsterState.Guard:
                UpdateGuard();
                break;
            case MonsterState.Attack:
                UpdateAttack();
                break;
        }

        // Debug
        if (showGizmos)
        {
            DebugDrawVision();
        }
    }

    // ==================== STATE MACHINE ====================
    public void SetState(MonsterState newState)
    {
        if (currentState == newState) return;

        // Save current state BEFORE changing (for stateBeforeChase)
        MonsterState previousState = currentState;

        OnExitState(currentState);

        currentState = newState;

        OnEnterState(newState, previousState);

        if (showDebugLogs)
        {
            Debug.Log($"MonsterAI: {previousState} -> {newState}");
        }
    }

    private void OnEnterState(MonsterState state, MonsterState previousState = MonsterState.Patrol)
    {
        switch (state)
        {
            case MonsterState.Patrol:
                agent.speed = patrolSpeed;
                agent.isStopped = false;
                isRecordingTrail = false; // Stop recording when patrolling
                GoToNextPatrolPoint();
                break;

            case MonsterState.Alert:
                agent.isStopped = true;
                isScreaming = true;
                alertPhase = AlertPhase.Screaming; // FIX: Start in screaming phase
                alertTimer = alertScreamDuration;
                // Start recording player trail from this moment
                StartRecordingTrail();
                // TODO: Play scream animation
                OnAlertScream();
                break;

            case MonsterState.Investigate:
                agent.isStopped = true;
                isScreaming = true;
                investigateTimer = investigateScreamDuration;
                // TODO: Play investigate scream animation
                OnInvestigateScream();
                break;

            case MonsterState.Trace:
                agent.isStopped = false;
                trailPointsFollowed = 0;
                traceTimer = 0f; // Reset speed timer

                // FIX: Check if trail has points before tracing
                if (playerTrail.Count == 0)
                {
                    if (showDebugLogs)
                    {
                        Debug.Log("MonsterAI: No trail points recorded, going to Search");
                    }
                    isTraceFromChaseLoss = false; // Reset flag
                    SetState(MonsterState.Search);
                    return;
                }

                // Determine starting index based on where trace is coming from
                if (isTraceFromChaseLoss)
                {
                    // After losing player in chase - start from END of trail (recent points)
                    currentTrailIndex = Mathf.Max(0, playerTrail.Count - traceAfterLoseChase);
                    isTraceFromChaseLoss = false; // Reset flag

                    if (showDebugLogs)
                    {
                        Debug.Log($"MonsterAI: Chase loss trace - starting from point {currentTrailIndex} of {playerTrail.Count}");
                    }
                }
                else
                {
                    // Normal trace (from flower) - start from BEGINNING
                    currentTrailIndex = 0;
                }

                if (trailPointsToFollow > 0)
                {
                    // LIMITED TRACE (Night 1 Flower 1, Night 3 Flower 1, or after chase loss)
                    // Walk speed, stop recording, follow exactly X points
                    agent.speed = patrolSpeed;
                    isRecordingTrail = false;

                    if (showDebugLogs)
                    {
                        Debug.Log($"MonsterAI: Limited trace - will follow {trailPointsToFollow} points at walk speed");
                    }
                }
                else
                {
                    // UNLIMITED TRACE (Last flower, Night 2 Flower 1, Night 3 Flower 2)
                    // Start at walk speed, speed up over time, keep recording
                    agent.speed = patrolSpeed;
                    isRecordingTrail = true;

                    if (showDebugLogs)
                    {
                        Debug.Log($"MonsterAI: Unlimited trace - will speed up over {traceSpeedUpTime}s (max speed: {maxTraceSpeed})");
                    }
                }

                GoToNextTrailPoint();
                break;

            case MonsterState.Search:
                agent.isStopped = true;
                searchTimer = searchDuration;
                isRecordingTrail = false; // Stop recording during search
                // TODO: Play search animation
                OnSearchStart();
                break;

            case MonsterState.Chase:
                // Remember what state we were in before chase
                stateBeforeChase = previousState;
                if (previousState == MonsterState.Guard || wasInGuardMode)
                {
                    wasInGuardMode = true;
                }

                agent.speed = chaseSpeed;
                losePlayerTimer = losePlayerTime;
                isReacting = true;
                reactionTimer = reactionDelay;
                agent.isStopped = true;

                // Keep recording trail during chase
                if (!isRecordingTrail)
                {
                    StartRecordingTrail();
                }

                // TODO: Play chase start sound/animation
                OnChaseStart();
                break;

            case MonsterState.Guard:
                agent.speed = patrolSpeed;
                agent.isStopped = false;
                wasInGuardMode = true; // FIX: Mark that we're in guard mode
                isRecordingTrail = false; // Stop recording when guarding
                SetupGuardPoints();
                GoToNextGuardPoint();
                break;

            case MonsterState.Attack:
                agent.isStopped = true;
                isAttacking = true;
                attackTimer = attackAnimationDuration;
                // Play attack animation (swing)
                OnAttackSwing();
                break;
        }
    }

    private void OnExitState(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Patrol:
                isWaitingAtPoint = false;
                break;

            case MonsterState.Alert:
                isScreaming = false;
                alertPhase = AlertPhase.Screaming; // Reset phase
                break;

            case MonsterState.Investigate:
                isScreaming = false;
                break;

            case MonsterState.Trace:
                // Don't clear trail here - might need it again
                break;

            case MonsterState.Search:
                break;

            case MonsterState.Chase:
                isReacting = false;
                agent.isStopped = false;
                // Stop chase music when leaving chase state
                OnChaseEnd();
                break;

            case MonsterState.Guard:
                // FIX: Don't clear guardPoints here - we need to know we were guarding
                // guardPoints will be cleared when entering Patrol
                break;

            case MonsterState.Attack:
                isAttacking = false;
                agent.isStopped = false;
                break;
        }
    }

    public MonsterState GetCurrentState()
    {
        return currentState;
    }

    // ==================== VISION SYSTEM ====================
    private void CheckVision()
    {
        if (player == null) return;

        canSeePlayer = false;

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 playerPosition = player.position + Vector3.up * 1f;
        Vector3 directionToPlayer = playerPosition - eyePosition;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Range check
        if (distanceToPlayer > visionRange) return;

        // FOV check
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > visionAngle / 2f) return;

        // Line of sight check
        RaycastHit hit;
        if (Physics.Raycast(eyePosition, directionToPlayer.normalized, out hit, distanceToPlayer, obstacleLayer | playerLayer))
        {
            if (hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
                lastKnownPlayerPosition = player.position;
                OnPlayerSpotted();
            }
        }
    }

    private void OnPlayerSpotted()
    {
        // If in any state except Attack and Chase, switch to Chase
        if (currentState != MonsterState.Attack && currentState != MonsterState.Chase)
        {
            SetState(MonsterState.Chase);
        }
    }

    // ==================== PATROL STATE ====================
    private void UpdatePatrol()
    {
        if (patrolPoints.Length == 0) return;

        // FIX: Clear guard mode flag and points when patrolling
        wasInGuardMode = false;
        if (guardPoints.Count > 0)
        {
            guardPoints.Clear();
            guardFlowerPosition = Vector3.zero;
        }

        if (isWaitingAtPoint)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaitingAtPoint = false;
                GoToNextPatrolPoint();
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartWaitingAtPoint();
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        int newIndex;
        do
        {
            newIndex = Random.Range(0, patrolPoints.Length);
        } while (newIndex == currentPatrolIndex && patrolPoints.Length > 1);

        currentPatrolIndex = newIndex;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private void StartWaitingAtPoint()
    {
        isWaitingAtPoint = true;
        waitTimer = waitTimeAtPoint;
    }

    // ==================== ALERT STATE ====================
    // FIX: Completely rewritten to use phases properly
    private void UpdateAlert()
    {
        if (alertPhase == AlertPhase.Screaming)
        {
            // Phase 1: Screaming in place
            alertTimer -= Time.deltaTime;

            if (alertTimer <= 0)
            {
                // Transition to rushing phase
                isScreaming = false;
                alertPhase = AlertPhase.Rushing;
                agent.isStopped = false;
                agent.speed = rushSpeed;
                agent.SetDestination(targetFlowerPosition);

                if (showDebugLogs)
                {
                    Debug.Log($"MonsterAI: Alert scream finished, rushing to flower at {targetFlowerPosition}");
                }
            }
        }
        else // AlertPhase.Rushing
        {
            // Phase 2: Rushing to flower
            // Check if arrived at flower
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
            {
                if (showDebugLogs)
                {
                    Debug.Log("MonsterAI: Arrived at flower position, investigating");
                }
                SetState(MonsterState.Investigate);
            }
        }
    }

    // ==================== INVESTIGATE STATE ====================
    private void UpdateInvestigate()
    {
        investigateTimer -= Time.deltaTime;

        // Look around (rotate) after initial scream
        if (investigateTimer <= investigateScreamDuration * 0.5f)
        {
            isScreaming = false;
            transform.Rotate(Vector3.up, 90f * Time.deltaTime);
        }

        if (investigateTimer <= 0)
        {
            // Decide what to do next based on night and flower count
            DecideAfterInvestigate();
        }
    }

    private void DecideAfterInvestigate()
    {
        if (showDebugLogs)
        {
            Debug.Log($"MonsterAI: DecideAfterInvestigate - Night {currentNight}, Flowers collected {flowersCollectedThisNight}/{totalFlowersThisNight}, IsLast: {isLastFlower}");
        }

        if (isLastFlower)
        {
            // Last flower - trace until caught or escaped
            trailPointsToFollow = -1; // Follow all points
            if (showDebugLogs) Debug.Log("MonsterAI: -> LAST FLOWER path, trailPointsToFollow = -1");
            SetState(MonsterState.Trace);
        }
        else if (currentNight == 1)
        {
            // Night 1, Flower 1: Trace 3 points only
            trailPointsToFollow = 3;
            if (showDebugLogs) Debug.Log("MonsterAI: -> NIGHT 1 path, trailPointsToFollow = 3");
            SetState(MonsterState.Trace);
        }
        else if (currentNight == 2)
        {
            if (flowersCollectedThisNight == 1)
            {
                // Night 2, Flower 1: Trace until sees player
                trailPointsToFollow = -1;
                if (showDebugLogs) Debug.Log("MonsterAI: -> NIGHT 2 FLOWER 1 path, trailPointsToFollow = -1");
                SetState(MonsterState.Trace);
            }
            else if (flowersCollectedThisNight == 2)
            {
                // Night 2, Flower 2: Guard a REMAINING flower (not the one just collected)
                Vector3 remainingFlowerPos = GetRandomRemainingFlowerPosition();
                if (remainingFlowerPos != Vector3.zero)
                {
                    guardFlowerPosition = remainingFlowerPos;
                    if (showDebugLogs) Debug.Log($"MonsterAI: -> NIGHT 2 FLOWER 2 path, guarding remaining flower at {guardFlowerPosition}");
                    SetState(MonsterState.Guard);
                }
                else
                {
                    // No remaining flowers found, just trace
                    trailPointsToFollow = -1;
                    if (showDebugLogs) Debug.Log("MonsterAI: -> NIGHT 2 FLOWER 2 path, no remaining flowers, tracing instead");
                    SetState(MonsterState.Trace);
                }
            }
        }
        else if (currentNight == 3)
        {
            if (flowersCollectedThisNight == 1)
            {
                // Night 3, Flower 1: Trace 3 points
                trailPointsToFollow = 3;
                if (showDebugLogs) Debug.Log("MonsterAI: -> NIGHT 3 FLOWER 1 path, trailPointsToFollow = 3");
                SetState(MonsterState.Trace);
            }
            else if (flowersCollectedThisNight == 2)
            {
                // Night 3, Flower 2: Trace until sees player
                trailPointsToFollow = -1;
                if (showDebugLogs) Debug.Log("MonsterAI: -> NIGHT 3 FLOWER 2 path, trailPointsToFollow = -1");
                SetState(MonsterState.Trace);
            }
            else if (flowersCollectedThisNight == 3)
            {
                // Night 3, Flower 3: Guard a REMAINING flower (not the one just collected)
                Vector3 remainingFlowerPos = GetRandomRemainingFlowerPosition();
                if (remainingFlowerPos != Vector3.zero)
                {
                    guardFlowerPosition = remainingFlowerPos;
                    if (showDebugLogs) Debug.Log($"MonsterAI: -> NIGHT 3 FLOWER 3 path, guarding remaining flower at {guardFlowerPosition}");
                    SetState(MonsterState.Guard);
                }
                else
                {
                    // No remaining flowers found, just trace
                    trailPointsToFollow = -1;
                    if (showDebugLogs) Debug.Log("MonsterAI: -> NIGHT 3 FLOWER 3 path, no remaining flowers, tracing instead");
                    SetState(MonsterState.Trace);
                }
            }
        }
        else
        {
            // Fallback - should not happen
            if (showDebugLogs) Debug.LogWarning($"MonsterAI: -> NO MATCHING PATH! Night={currentNight}, defaulting to patrol");
            SetState(MonsterState.Patrol);
        }
    }

    /// <summary>
    /// Gets the position of a random remaining (not yet collected) flower
    /// </summary>
    private Vector3 GetRandomRemainingFlowerPosition()
    {
        if (FlowerSpawnManager.Instance == null)
        {
            Debug.LogWarning("MonsterAI: FlowerSpawnManager not found!");
            return Vector3.zero;
        }

        List<GameObject> remainingFlowers = FlowerSpawnManager.Instance.GetSpawnedFlowers();

        if (remainingFlowers == null || remainingFlowers.Count == 0)
        {
            Debug.LogWarning("MonsterAI: No remaining flowers to guard!");
            return Vector3.zero;
        }

        // Pick a random remaining flower
        GameObject randomFlower = remainingFlowers[Random.Range(0, remainingFlowers.Count)];

        if (randomFlower != null)
        {
            if (showDebugLogs)
            {
                Debug.Log($"MonsterAI: Selected remaining flower '{randomFlower.name}' at {randomFlower.transform.position} to guard");
            }
            return randomFlower.transform.position;
        }

        return Vector3.zero;
    }

    // ==================== TRACE STATE ====================
    private void UpdateTrace()
    {
        // Check if trail is empty
        if (playerTrail.Count == 0)
        {
            if (showDebugLogs)
            {
                Debug.Log("MonsterAI: Trail is empty, going to Search");
            }
            SetState(MonsterState.Search);
            return;
        }

        // === LIMITED TRACE: Check if we've followed enough points ===
        if (trailPointsToFollow > 0 && trailPointsFollowed >= trailPointsToFollow)
        {
            if (showDebugLogs)
            {
                Debug.Log($"MonsterAI: Reached trace limit ({trailPointsFollowed}/{trailPointsToFollow} points), going to Search");
            }
            SetState(MonsterState.Search);
            return;
        }

        // === Check if reached end of trail ===
        if (currentTrailIndex >= playerTrail.Count)
        {
            if (trailPointsToFollow == -1)
            {
                // UNLIMITED TRACE - reached end of trail, player escaped
                if (showDebugLogs)
                {
                    Debug.Log($"MonsterAI: Reached end of trail (unlimited trace), going to Search");
                }
                SetState(MonsterState.Search);
                return;
            }
            else
            {
                // LIMITED TRACE - trail is shorter than limit, we're done
                if (showDebugLogs)
                {
                    Debug.Log($"MonsterAI: Trail ended early ({trailPointsFollowed} points), going to Search");
                }
                SetState(MonsterState.Search);
                return;
            }
        }

        // === UNLIMITED TRACE: Speed up over time ===
        if (trailPointsToFollow == -1)
        {
            traceTimer += Time.deltaTime;

            // Calculate speed: lerp from patrolSpeed to maxTraceSpeed over traceSpeedUpTime
            float speedProgress = Mathf.Clamp01(traceTimer / traceSpeedUpTime);
            agent.speed = Mathf.Lerp(patrolSpeed, maxTraceSpeed, speedProgress);

            // Debug speed every 10 seconds
            if (showDebugLogs && Mathf.FloorToInt(traceTimer) % 10 == 0 && Mathf.FloorToInt(traceTimer - Time.deltaTime) % 10 != 0)
            {
                Debug.Log($"MonsterAI: Trace speed: {agent.speed:F1} ({speedProgress * 100:F0}% of max)");
            }
        }

        // === Move to next trail point when arrived ===
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (showDebugLogs)
            {
                Debug.Log($"MonsterAI: Reached trail point {currentTrailIndex}. Followed: {trailPointsFollowed}, Trail count: {playerTrail.Count}");
            }

            currentTrailIndex++;
            trailPointsFollowed++;

            // Go to next point if available
            if (currentTrailIndex < playerTrail.Count)
            {
                GoToNextTrailPoint();
            }
        }
    }

    private void StartRecordingTrail()
    {
        isRecordingTrail = true;
        playerTrail.Clear();
        recordedTrailPoints.Clear();
        currentTrailIndex = 0;

        if (showDebugLogs)
        {
            Debug.Log("MonsterAI: Started recording player trail");
        }
    }

    private void StopRecordingTrail()
    {
        isRecordingTrail = false;

        if (showDebugLogs)
        {
            Debug.Log($"MonsterAI: Stopped recording trail. Total points: {playerTrail.Count}");
        }
    }

    private void RecordPlayerTrail()
    {
        if (player == null || patrolPoints.Length == 0) return;

        // Check if player is near any patrol point
        foreach (Transform point in patrolPoints)
        {
            if (point == null) continue;

            float distance = Vector3.Distance(player.position, point.position);
            if (distance <= trailRecordDistance)
            {
                // Only record if not already recorded
                if (!recordedTrailPoints.Contains(point))
                {
                    recordedTrailPoints.Add(point);
                    playerTrail.Add(point);

                    if (showDebugLogs)
                    {
                        Debug.Log($"MonsterAI: Trail point recorded - {point.name}. Total: {playerTrail.Count}");
                    }
                }
            }
        }
    }

    private void GoToNextTrailPoint()
    {
        if (currentTrailIndex >= playerTrail.Count)
        {
            return;
        }

        Transform targetPoint = playerTrail[currentTrailIndex];
        if (targetPoint != null)
        {
            agent.SetDestination(targetPoint.position);
        }
    }

    // ==================== SEARCH STATE ====================
    private void UpdateSearch()
    {
        searchTimer -= Time.deltaTime;

        // Look around animation (rotate in place)
        transform.Rotate(Vector3.up, 120f * Time.deltaTime);

        if (searchTimer <= 0)
        {
            // Decide what to do after search
            DecideAfterSearch();
        }
    }

    private void DecideAfterSearch()
    {
        // FIX: Check wasInGuardMode flag instead of guardPoints.Count
        if (wasInGuardMode && guardFlowerPosition != Vector3.zero)
        {
            if (showDebugLogs)
            {
                Debug.Log("MonsterAI: Was in guard mode, returning to Guard");
            }
            SetState(MonsterState.Guard);
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.Log("MonsterAI: Search complete, returning to Patrol");
            }
            SetState(MonsterState.Patrol);
        }
    }

    // ==================== CHASE STATE ====================
    private void UpdateChase()
    {
        // Reaction delay
        if (isReacting)
        {
            reactionTimer -= Time.deltaTime;
            if (reactionTimer <= 0)
            {
                isReacting = false;
                agent.isStopped = false;
            }
            return;
        }

        if (canSeePlayer)
        {
            agent.SetDestination(player.position);
            losePlayerTimer = losePlayerTime;

            // FIX: Null check before distance calculation
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackRange)
                {
                    SetState(MonsterState.Attack);
                    return;
                }
            }
        }
        else
        {
            // Lost sight - go to last known position
            agent.SetDestination(lastKnownPlayerPosition);

            losePlayerTimer -= Time.deltaTime;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                losePlayerTimer -= Time.deltaTime * 2f;
            }

            if (losePlayerTimer <= 0)
            {
                OnPlayerLost();
            }
        }
    }

    private void OnPlayerLost()
    {
        if (showDebugLogs)
        {
            Debug.Log("MonsterAI: Lost player, starting trace from last known position");
        }

        // After losing player, trace for limited points from where player was last seen
        trailPointsToFollow = traceAfterLoseChase;

        // Set flag so OnEnterState knows to start from end of trail, not beginning
        isTraceFromChaseLoss = true;

        SetState(MonsterState.Trace);
    }

    // ==================== GUARD STATE ====================
    private void UpdateGuard()
    {
        if (guardPoints.Count == 0)
        {
            // If no guard points, setup again
            SetupGuardPoints();
            if (guardPoints.Count == 0)
            {
                // Still no points, go to patrol
                SetState(MonsterState.Patrol);
                return;
            }
        }

        if (isWaitingAtPoint)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaitingAtPoint = false;
                GoToNextGuardPoint();
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartWaitingAtPoint();
        }
    }

    private void SetupGuardPoints()
    {
        guardPoints.Clear();

        if (patrolPoints.Length == 0 || guardFlowerPosition == Vector3.zero)
        {
            Debug.LogWarning("MonsterAI: Cannot setup guard points - no patrol points or flower position");
            return;
        }

        // Find the closest patrol points to the flower position
        List<Transform> sortedPoints = new List<Transform>(patrolPoints);
        sortedPoints.Sort((a, b) =>
        {
            float distA = Vector3.Distance(a.position, guardFlowerPosition);
            float distB = Vector3.Distance(b.position, guardFlowerPosition);
            return distA.CompareTo(distB);
        });

        // Take the closest N points
        int pointsToTake = Mathf.Min(guardPointCount, sortedPoints.Count);
        for (int i = 0; i < pointsToTake; i++)
        {
            guardPoints.Add(sortedPoints[i]);
        }

        if (showDebugLogs)
        {
            Debug.Log($"MonsterAI: Guard mode setup with {guardPoints.Count} points near flower at {guardFlowerPosition}");
        }
    }

    private void GoToNextGuardPoint()
    {
        if (guardPoints.Count == 0) return;

        currentGuardIndex = (currentGuardIndex + 1) % guardPoints.Count;
        agent.SetDestination(guardPoints[currentGuardIndex].position);
    }

    // ==================== ATTACK STATE ====================
    private void UpdateAttack()
    {
        // Null check
        if (player == null)
        {
            SetState(MonsterState.Patrol);
            return;
        }

        // Look at player during attack
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDirection),
                Time.deltaTime * 10f
            );
        }

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            // Attack animation finished - check if player was hit
            if (attackTimer <= 0)
            {
                isAttacking = false;

                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (showDebugLogs)
                {
                    Debug.Log($"MonsterAI: Attack finished! Distance to player: {distanceToPlayer:F2}, Hit range: {attackHitRange}");
                }

                if (distanceToPlayer <= attackHitRange)
                {
                    // Player was hit! Trigger jumpscare
                    if (showDebugLogs) Debug.Log("MonsterAI: Player HIT! Triggering jumpscare...");
                    OnAttackHit();
                }
                else
                {
                    // Player escaped! Continue chase
                    if (showDebugLogs) Debug.Log("MonsterAI: Player ESCAPED! Continuing chase...");
                    SetState(MonsterState.Chase);
                }
            }
        }
    }

    // ==================== PUBLIC METHODS (Called by other scripts) ====================

    /// <summary>
    /// Called by FlowerPickup/GameManager when a flower is collected
    /// </summary>
    public void OnFlowerCollected(Vector3 flowerPosition, bool isLast)
    {
        if (showDebugLogs)
        {
            Debug.Log($"MonsterAI: Flower collected at {flowerPosition}. IsLast: {isLast}");
        }

        flowersCollectedThisNight++;
        isLastFlower = isLast;
        targetFlowerPosition = flowerPosition;

        // FIX: If currently tracing, stop but DON'T clear the trail yet
        // The new Alert state will clear it when it starts recording
        if (currentState == MonsterState.Trace)
        {
            StopRecordingTrail();
        }

        // If in Guard mode and this is the last flower
        if (currentState == MonsterState.Guard && isLast)
        {
            // Alert and rush to this flower
            SetState(MonsterState.Alert);
            return;
        }

        // Alert state - scream and rush to flower
        SetState(MonsterState.Alert);
    }

    /// <summary>
    /// Called by GameManager at the start of each night
    /// </summary>
    public void SetupForNight(int nightNumber, int totalFlowers)
    {
        currentNight = nightNumber;
        totalFlowersThisNight = totalFlowers;
        flowersCollectedThisNight = 0;
        isLastFlower = false;
        wasInGuardMode = false;

        // Clear any previous trail data
        playerTrail.Clear();
        recordedTrailPoints.Clear();
        isRecordingTrail = false;
        isTraceFromChaseLoss = false;
        currentTrailIndex = 0;
        trailPointsFollowed = 0;
        trailPointsToFollow = -1;
        traceTimer = 0f;

        // Clear guard data
        guardPoints.Clear();
        guardFlowerPosition = Vector3.zero;

        // Reset to patrol
        SetState(MonsterState.Patrol);

        if (showDebugLogs)
        {
            Debug.Log($"MonsterAI: Setup for Night {nightNumber} with {totalFlowers} flowers");
        }
    }

    /// <summary>
    /// Check if monster can currently see the player
    /// </summary>
    public bool CanSeePlayer()
    {
        return canSeePlayer;
    }

    /// <summary>
    /// Check if monster is currently chasing
    /// </summary>
    public bool IsChasing()
    {
        return currentState == MonsterState.Chase;
    }

    /// <summary>
    /// Force investigate a position (for external triggers)
    /// </summary>
    public void InvestigatePosition(Vector3 position)
    {
        targetFlowerPosition = position;
        SetState(MonsterState.Alert);
    }

    // ==================== ANIMATION/SOUND METHODS ====================
    // These methods call MonsterAnimationHandler and MonsterAudioManager

    protected virtual void OnAlertScream()
    {
        // Play scream animation
        if (MonsterAnimationHandler.Instance != null)
        {
            MonsterAnimationHandler.Instance.PlayScream();
        }

        // Play alert scream sound
        if (MonsterAudioManager.Instance != null)
        {
            MonsterAudioManager.Instance.PlayAlertScream();
        }

        if (showDebugLogs) Debug.Log("MonsterAI: Alert scream triggered");
    }

    protected virtual void OnInvestigateScream()
    {
        // Play scream animation
        if (MonsterAnimationHandler.Instance != null)
        {
            MonsterAnimationHandler.Instance.PlayScream();
        }

        // Play investigate scream sound
        if (MonsterAudioManager.Instance != null)
        {
            MonsterAudioManager.Instance.PlayInvestigateScream();
        }

        if (showDebugLogs) Debug.Log("MonsterAI: Investigate scream triggered");
    }

    protected virtual void OnSearchStart()
    {
        // Play search animation
        if (MonsterAnimationHandler.Instance != null)
        {
            MonsterAnimationHandler.Instance.PlaySearch();
        }

        // Play search sound
        if (MonsterAudioManager.Instance != null)
        {
            MonsterAudioManager.Instance.PlaySearchSound();
        }

        if (showDebugLogs) Debug.Log("MonsterAI: Search started");
    }

    protected virtual void OnChaseStart()
    {
        // Animation is handled automatically by speed (walk -> run blend)

        // Start chase music
        if (MonsterAudioManager.Instance != null)
        {
            MonsterAudioManager.Instance.StartChaseMusic();
        }

        if (showDebugLogs) Debug.Log("MonsterAI: Chase started - music playing");
    }

    protected virtual void OnChaseEnd()
    {
        // Stop chase music
        if (MonsterAudioManager.Instance != null)
        {
            MonsterAudioManager.Instance.StopChaseMusic();
        }

        if (showDebugLogs) Debug.Log("MonsterAI: Chase ended - music stopped");
    }

    /// <summary>
    /// Called when monster starts attack swing animation
    /// Player can still escape during this!
    /// </summary>
    protected virtual void OnAttackSwing()
    {
        // Play attack animation
        if (MonsterAnimationHandler.Instance != null)
        {
            MonsterAnimationHandler.Instance.PlayAttack();
        }

        // Play attack swing sound (optional - different from jumpscare sound)
        if (MonsterAudioManager.Instance != null)
        {
            MonsterAudioManager.Instance.PlayAttackSwingSound();
        }

        if (showDebugLogs) Debug.Log("MonsterAI: Attack swing started - player can escape!");
    }

    /// <summary>
    /// Called when attack hits the player (player didn't escape in time)
    /// </summary>
    protected virtual void OnAttackHit()
    {
        // Prevent multiple calls
        if (JumpscareManager.Instance != null && JumpscareManager.Instance.IsJumpscareActive())
        {
            return;
        }

        if (showDebugLogs) Debug.Log("MonsterAI: ATTACK HIT! Triggering jumpscare...");

        // Stop chase music before jumpscare
        if (MonsterAudioManager.Instance != null)
        {
            MonsterAudioManager.Instance.StopChaseMusic();
        }

        // Trigger jumpscare sequence
        if (JumpscareManager.Instance != null)
        {
            JumpscareManager.Instance.TriggerJumpscare();
        }
        else
        {
            // Fallback if JumpscareManager not found - call GameManager directly
            Debug.LogWarning("MonsterAI: JumpscareManager not found! Calling GameManager directly.");

            if (NightGameManager.Instance != null)
            {
                NightGameManager.Instance.OnPlayerCaughtByMonster();
            }
            else
            {
                GameObject gameManagerObj = GameObject.Find("GameManager");
                if (gameManagerObj != null)
                {
                    gameManagerObj.SendMessage("OnPlayerCaughtByMonster", SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    Debug.Log("MonsterAI: [TEST MODE] Player caught! No manager found.");
                }
            }
        }
    }

    // ==================== DEBUG VISUALIZATION ====================
    private void DebugDrawVision()
    {
        if (!Application.isEditor) return;

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        // Vision range
        Debug.DrawRay(eyePosition, transform.forward * visionRange, canSeePlayer ? Color.red : Color.yellow);

        // FOV cone
        Vector3 leftDir = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;
        Debug.DrawRay(eyePosition, leftDir * visionRange, Color.blue);
        Debug.DrawRay(eyePosition, rightDir * visionRange, Color.blue);

        // Line to player if visible
        if (canSeePlayer && player != null)
        {
            Debug.DrawLine(eyePosition, player.position + Vector3.up, Color.red);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        // Vision range
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // FOV cone
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 leftDir = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(eyePosition, eyePosition + leftDir * visionRange);
        Gizmos.DrawLine(eyePosition, eyePosition + rightDir * visionRange);

        // Target flower position
        if (targetFlowerPosition != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(targetFlowerPosition, 1f);
        }

        // Guard flower position
        if (guardFlowerPosition != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(guardFlowerPosition, 1.5f);
        }

        // Trail points
        if (playerTrail != null && playerTrail.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < playerTrail.Count; i++)
            {
                if (playerTrail[i] != null)
                {
                    Gizmos.DrawSphere(playerTrail[i].position, 0.3f);
                    if (i > 0 && playerTrail[i - 1] != null)
                    {
                        Gizmos.DrawLine(playerTrail[i - 1].position, playerTrail[i].position);
                    }
                }
            }

            // Current trail target
            if (currentTrailIndex < playerTrail.Count && playerTrail[currentTrailIndex] != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(playerTrail[currentTrailIndex].position, 0.5f);
            }
        }

        // Guard points
        if (guardPoints != null && guardPoints.Count > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in guardPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireCube(point.position, Vector3.one * 0.8f);
                }
            }
        }

        // Trail record distance (around patrol points)
        if (showGizmos && patrolPoints != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, trailRecordDistance);
                }
            }
        }
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Trigger Alert (Fake Flower)")]
    public void TestTriggerAlert()
    {
        if (player != null)
        {
            OnFlowerCollected(player.position + Vector3.forward * 5f, false);
        }
    }

    [ContextMenu("Test: Trigger Last Flower Alert")]
    public void TestTriggerLastFlower()
    {
        if (player != null)
        {
            OnFlowerCollected(player.position + Vector3.forward * 5f, true);
        }
    }

    [ContextMenu("Test: Force Chase")]
    public void TestForceChase()
    {
        SetState(MonsterState.Chase);
    }

    [ContextMenu("Test: Setup Night 1")]
    public void TestSetupNight1()
    {
        SetupForNight(1, 2);
    }

    [ContextMenu("Test: Setup Night 2")]
    public void TestSetupNight2()
    {
        SetupForNight(2, 3);
    }

    [ContextMenu("Test: Setup Night 3")]
    public void TestSetupNight3()
    {
        SetupForNight(3, 4);
    }
}
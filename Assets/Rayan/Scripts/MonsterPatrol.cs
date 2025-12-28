using UnityEngine;
using UnityEngine.AI;

public class MonsterPatrol : MonoBehaviour
{
    public enum MonsterState
    {
        Patrol,
        Chase,
        Attack
        // Future states you can add:   GuardMode, TraceChase, AngerScream, RushToLocation,
    }

    // ==================== INSPECTOR SETTINGS ====================
    [Header("=== PATROL SETTINGS ===")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float waitTimeAtPoint = 2f;

    [Header("=== CHASE SETTINGS ===")]
    public float chaseSpeed = 5f;
    public float reactionDelay = 0.5f; // Time before monster starts chasing (gives player a chance)

    [Header("=== VISION SETTINGS ===")]
    public float visionRange = 15f;
    public float visionAngle = 90f; // Field of view angle
    public float eyeHeight = 1.5f; // Height of monster's "eyes"
    public LayerMask playerLayer;
    public LayerMask obstacleLayer; // For walls/bushes that block vision

    [Header("=== CHASE LOSS SETTINGS ===")]
    public float losePlayerTime = 3f; // Time to lose player after breaking line of sight

    [Header("=== REFERENCES ===")]
    public Transform player; // Assign in inspector or find automatically

    [Header("=== ATTACK SETTINGS ===")]
    public float attackWindupTime = 0.3f; // Time before attack lands

    // ==================== PRIVATE VARIABLES ====================
    // Components
    private NavMeshAgent agent;

    // State Machine
    private MonsterState currentState = MonsterState.Patrol;

    // Patrol variables
    private int currentPatrolIndex;
    private float waitTimer;
    private bool isWaitingAtPoint = false;

    // Chase variables
    private float losePlayerTimer;
    private bool canSeePlayer = false;
    private Vector3 lastKnownPlayerPosition;
    private float reactionTimer;
    private bool isReacting = false; // True during reaction delay
    private bool isAttackWindup = false;
    private float attackWindupTimer;

    // ==================== METHODS ====================
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Auto-find player if not assigned
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

        // Start patrolling
        SetState(MonsterState.Patrol);
    }

    void Update()
    {
        // Always check vision
        CheckVision();

        // Run current state logic
        switch (currentState)
        {
            case MonsterState.Patrol:
                UpdatePatrol();
                break;
            case MonsterState.Chase:
                UpdateChase();
                break;
            case MonsterState.Attack:
                UpdateAttack();
                break;
        }

        // Debug info
        DebugDrawVision();
    }

    // ==================== STATE MACHINE ====================
    /// Changes the monster's state and handles transitions.
    public void SetState(MonsterState newState)
    {
        // Exit current state
        OnExitState(currentState);

        // Change state
        MonsterState previousState = currentState;
        currentState = newState;

        // Enter new state
        OnEnterState(newState);

        Debug.Log($"Monster State: {previousState} -> {newState}");
    }

    /// <summary>
    /// Called when entering a new state. Setup logic goes here.
    /// </summary>
    private void OnEnterState(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Patrol:
                agent.speed = patrolSpeed;
                GoToNextPatrolPoint();
                break;

            case MonsterState.Chase:
                agent.speed = chaseSpeed;
                losePlayerTimer = losePlayerTime;
                // Start with reaction delay
                isReacting = true;
                reactionTimer = reactionDelay;
                agent.isStopped = true; // Stop briefly during reaction
                break;

            case MonsterState.Attack:
                agent.isStopped = true;
                isAttackWindup = true;
                attackWindupTimer = attackWindupTime;
                // Play warning sound/animation here
                break;
        }
    }

    /// <summary>
    /// Called when exiting a state. Cleanup logic goes here.
    /// </summary>
    private void OnExitState(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Patrol:
                isWaitingAtPoint = false;
                break;

            case MonsterState.Chase:
                isReacting = false;
                agent.isStopped = false;
                break;

            case MonsterState.Attack:
                agent.isStopped = false;
                break;
        }
    }

    /// <summary>
    /// Get the current state (useful for other scripts)
    /// </summary>
    public MonsterState GetCurrentState()
    {
        return currentState;
    }

    // ==================== VISION SYSTEM ====================
    /// <summary>
    /// Checks if the monster can see the player using raycasts and FOV.
    /// </summary>
    private void CheckVision()
    {
        if (player == null) return;

        canSeePlayer = false;

        // Get direction and distance to player
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 playerPosition = player.position + Vector3.up * 1f; // Player's chest height
        Vector3 directionToPlayer = playerPosition - eyePosition;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Check 1: Is player within range?
        if (distanceToPlayer > visionRange)
        {
            return;
        }

        // Check 2: Is player within field of view angle?
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > visionAngle / 2f)
        {
            return;
        }

        // Check 3: Is there a clear line of sight? (No walls/bushes blocking)
        RaycastHit hit;
        if (Physics.Raycast(eyePosition, directionToPlayer.normalized, out hit, distanceToPlayer, obstacleLayer | playerLayer))
        {
            // Check if we hit the player (not an obstacle)
            if (hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
                lastKnownPlayerPosition = player.position;

                // React to seeing player
                OnPlayerSpotted();
            }
        }
    }

    /// <summary>
    /// Called when the monster spots the player.
    /// </summary>
    private void OnPlayerSpotted()
    {
        // If patrolling, start chasing
        if (currentState == MonsterState.Patrol)
        {
            SetState(MonsterState.Chase);
        }
    }

    // ==================== PATROL STATE ====================
    private void UpdatePatrol()
    {
        if (patrolPoints.Length == 0) return;

        // If waiting at a point
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

        // Check if reached patrol point
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartWaitingAtPoint();
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        // Pick random patrol point (different from current)
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

    // ==================== CHASE STATE ====================
    private void UpdateChase()
    {
        // Handle reaction delay (gives player a chance to react)
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

        // If can see player, update chase target
        if (canSeePlayer)
        {
            agent.SetDestination(player.position);
            losePlayerTimer = losePlayerTime; // Reset lose timer

            // Check if close enough to attack
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= agent.stoppingDistance + 0.5f)
            {
                SetState(MonsterState.Attack);
                return;
            }
        }
        else
        {
            // Lost sight of player - go to last known position
            agent.SetDestination(lastKnownPlayerPosition);

            // Count down lose timer
            losePlayerTimer -= Time.deltaTime;

            // Check if reached last known position
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // Player not here, give up faster
                losePlayerTimer -= Time.deltaTime * 2f;
            }

            // Lost player completely
            if (losePlayerTimer <= 0)
            {
                OnPlayerLost();
            }
        }
    }

    /// <summary>
    /// Called when the monster loses track of the player.
    /// </summary>
    private void OnPlayerLost()
    {
        Debug.Log("Monster lost the player, returning to patrol.");
        SetState(MonsterState.Patrol);

        // Future: You can add Trace Chase or other behaviors here
    }

    // ==================== ATTACK STATE ====================
    // Modify UpdateAttack
    private void UpdateAttack()
    {
        // Look at player
        if (player != null)
        {
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
        }

        // Attack windup - gives player a tiny chance to escape
        if (isAttackWindup)
        {
            attackWindupTimer -= Time.deltaTime;

            // Check if player escaped during windup!
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > agent.stoppingDistance + 1.5f)
            {
                // Player escaped! Go back to chasing
                isAttackWindup = false;
                SetState(MonsterState.Chase);
                return;
            }

            if (attackWindupTimer <= 0)
            {
                isAttackWindup = false;
                OnAttackPlayer(); // NOW the attack happens
            }
            return;
        }
    }

    /// <summary>
    /// Called when monster attacks the player.
    /// Override or extend this for jumpscare logic.
    /// </summary>
    protected virtual void OnAttackPlayer()
    {
        Debug.Log("ATTACK! Jumpscare would trigger here.");

        // Notify the GameManager or PlayerController
        // Option 1: Send message
        // player.SendMessage("OnCaughtByMonster", SendMessageOptions.DontRequireReceiver);

        // Option 2: Direct reference (you'll add this later)
        // GameManager.Instance.TriggerJumpscare();

        // Option 3: Event system (most flexible - you'll add this later)
        // OnPlayerCaught?.Invoke();

        // For now, just pause the monster
        // The game manager will handle the actual jumpscare
    }

    // ==================== PUBLIC METHODS ====================
    /// <summary>
    /// Force the monster to investigate a position (for stick collection events)
    /// </summary>
    public void InvestigatePosition(Vector3 position)
    {
        lastKnownPlayerPosition = position;
        // Future: SetState(MonsterState.RushToLocation);
        // For now, just set chase destination
        if (currentState == MonsterState.Patrol)
        {
            SetState(MonsterState.Chase);
        }
        agent.SetDestination(position);
    }

    /// <summary>
    /// Check if monster is currently chasing
    /// </summary>
    public bool IsChasing()
    {
        return currentState == MonsterState.Chase;
    }

    /// <summary>
    /// Check if monster can currently see the player
    /// </summary>
    public bool CanSeePlayer()
    {
        return canSeePlayer;
    }

    // ==================== DEBUG ====================
    private void DebugDrawVision()
    {
        if (!Application.isEditor) return;

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        // Draw vision range
        Debug.DrawRay(eyePosition, transform.forward * visionRange, canSeePlayer ? Color.red : Color.yellow);

        // Draw FOV cone edges
        Vector3 leftDir = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;
        Debug.DrawRay(eyePosition, leftDir * visionRange, Color.blue);
        Debug.DrawRay(eyePosition, rightDir * visionRange, Color.blue);

        // Draw line to player if visible
        if (canSeePlayer && player != null)
        {
            Debug.DrawLine(eyePosition, player.position + Vector3.up, Color.red);
        }
    }

    // Draw gizmos in Scene view
    private void OnDrawGizmosSelected()
    {
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        // Vision range sphere
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // FOV cone visualization
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 leftDir = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(eyePosition, eyePosition + leftDir * visionRange);
        Gizmos.DrawLine(eyePosition, eyePosition + rightDir * visionRange);

        // Last known player position
        if (Application.isPlaying && lastKnownPlayerPosition != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(lastKnownPlayerPosition, 0.5f);
        }
    }
}
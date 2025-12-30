using UnityEngine;
using UnityEngine.AI;

// ================================================================================
// MONSTER ANIMATION HANDLER
// ================================================================================
// Controls all monster animations based on AI state and movement speed.
// 
// SETUP INSTRUCTIONS:
// 1. Add this script to your Monster GameObject (same object as MonsterAI)
// 2. Make sure Monster has an Animator component
// 3. Create an Animator Controller with these parameters:
//    - "Speed" (Float) - Controls walk/run blend
//    - "IsMoving" (Bool) - True when monster is walking/running
//    - "Scream" (Trigger) - Triggers scream animation
//    - "Search" (Trigger) - Triggers search animation
//    - "Attack" (Trigger) - Triggers attack animation
//
// ANIMATOR CONTROLLER SETUP:
// 1. Create a Blend Tree for movement:
//    - Parameter: Speed
//    - Threshold 0: Idle
//    - Threshold 2: Walk animation
//    - Threshold 5: Run animation
//    - Threshold 12: Sprint animation (optional)
//
// 2. Create transitions:
//    - Any State -> Scream (Trigger: Scream)
//    - Any State -> Search (Trigger: Search)
//    - Any State -> Attack (Trigger: Attack)
//    - Scream/Search/Attack -> Movement Blend Tree (when animation ends)
// ================================================================================

public class MonsterAnimationHandler : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static MonsterAnimationHandler Instance { get; private set; }

    // ==================== REFERENCES ====================
    [Header("=== REFERENCES ===")]
    [Tooltip("Animator component (auto-found if not set)")]
    public Animator animator;

    [Tooltip("NavMeshAgent for speed (auto-found if not set)")]
    public NavMeshAgent agent;

    [Tooltip("MonsterAI reference (auto-found if not set)")]
    public MonsterAI monsterAI;

    // ==================== ANIMATION SETTINGS ====================
    [Header("=== SPEED THRESHOLDS ===")]
    [Tooltip("Speed below this = idle")]
    public float idleThreshold = 0.1f;

    [Tooltip("Speed for normal walk animation")]
    public float walkSpeed = 2f;

    [Tooltip("Speed for run animation")]
    public float runSpeed = 5f;

    [Tooltip("Smoothing for speed transitions")]
    public float speedSmoothTime = 0.1f;

    // ==================== ANIMATOR PARAMETERS ===
    [Header("=== ANIMATOR PARAMETER NAMES ===")]
    [Tooltip("Float parameter for movement speed")]
    public string speedParameter = "Speed";

    [Tooltip("Bool parameter for is moving")]
    public string isMovingParameter = "IsMoving";

    [Tooltip("Trigger for scream animation")]
    public string screamTrigger = "Scream";

    [Tooltip("Trigger for search animation")]
    public string searchTrigger = "Search";

    [Tooltip("Trigger for attack animation")]
    public string attackTrigger = "Attack";

    // ==================== DEBUG ====================
    [Header("=== DEBUG ===")]
    public bool showDebugLogs = true;

    // ==================== PRIVATE VARIABLES ====================
    private float currentSpeed = 0f;
    private float speedVelocity = 0f; // For SmoothDamp
    private bool isPlayingSpecialAnimation = false;

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
            Debug.LogWarning("MonsterAnimationHandler: Multiple instances detected!");
            Destroy(this);
            return;
        }
    }

    void Start()
    {
        // Auto-find Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        // Auto-find NavMeshAgent
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // Auto-find MonsterAI
        if (monsterAI == null)
        {
            monsterAI = GetComponent<MonsterAI>();
        }

        // Validation
        if (animator == null)
        {
            Debug.LogError("MonsterAnimationHandler: No Animator found!");
        }

        if (showDebugLogs)
        {
            Debug.Log("MonsterAnimationHandler: Initialized");
        }
    }

    void Update()
    {
        if (animator == null) return;

        // Update movement animation based on speed
        UpdateMovementAnimation();
    }

    // ==================== MOVEMENT ANIMATION ====================
    private void UpdateMovementAnimation()
    {
        if (agent == null) return;

        // Get current actual speed
        float targetSpeed = agent.velocity.magnitude;

        // Smooth the speed transition
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, speedSmoothTime);

        // Set animator parameters
        animator.SetFloat(speedParameter, currentSpeed);
        animator.SetBool(isMovingParameter, currentSpeed > idleThreshold);
    }

    // ==================== SPECIAL ANIMATIONS ====================

    /// <summary>
    /// Play scream animation (Alert or Investigate)
    /// </summary>
    public void PlayScream()
    {
        if (animator == null) return;

        animator.SetTrigger(screamTrigger);
        isPlayingSpecialAnimation = true;

        if (showDebugLogs)
        {
            Debug.Log("MonsterAnimationHandler: Playing Scream animation");
        }
    }

    /// <summary>
    /// Play search/look around animation
    /// </summary>
    public void PlaySearch()
    {
        if (animator == null) return;

        animator.SetTrigger(searchTrigger);
        isPlayingSpecialAnimation = true;

        if (showDebugLogs)
        {
            Debug.Log("MonsterAnimationHandler: Playing Search animation");
        }
    }

    /// <summary>
    /// Play attack windup animation
    /// </summary>
    public void PlayAttack()
    {
        if (animator == null) return;

        animator.SetTrigger(attackTrigger);
        isPlayingSpecialAnimation = true;

        if (showDebugLogs)
        {
            Debug.Log("MonsterAnimationHandler: Playing Attack animation");
        }
    }

    /// <summary>
    /// Called when special animation ends (call from Animation Event or after duration)
    /// </summary>
    public void OnSpecialAnimationEnd()
    {
        isPlayingSpecialAnimation = false;

        if (showDebugLogs)
        {
            Debug.Log("MonsterAnimationHandler: Special animation ended");
        }
    }

    // ==================== ANIMATION EVENTS (Called from Animation Clips) ====================

    /// <summary>
    /// Called from walk animation when foot hits ground
    /// </summary>
    public void OnFootstepWalk()
    {
        if (MonsterAudioManager.Instance != null)
        {
            MonsterAudioManager.Instance.PlayWalkFootstep();
        }
    }

    /// <summary>
    /// Called from run animation when foot hits ground
    /// </summary>
    public void OnFootstepRun()
    {
        if (MonsterAudioManager.Instance != null)
        {
            MonsterAudioManager.Instance.PlayRunFootstep();
        }
    }

    /// <summary>
    /// Called at the peak of scream animation
    /// </summary>
    public void OnScreamPeak()
    {
        // Sound is handled by MonsterAI calling MonsterAudioManager directly
        if (showDebugLogs)
        {
            Debug.Log("MonsterAnimationHandler: Scream peak reached");
        }
    }

    /// <summary>
    /// Called when attack animation connects
    /// </summary>
    public void OnAttackHit()
    {
        if (showDebugLogs)
        {
            Debug.Log("MonsterAnimationHandler: Attack hit!");
        }
    }

    // ==================== PUBLIC HELPER METHODS ====================

    /// <summary>
    /// Check if currently playing a special (non-movement) animation
    /// </summary>
    public bool IsPlayingSpecialAnimation()
    {
        return isPlayingSpecialAnimation;
    }

    /// <summary>
    /// Get current movement speed
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    /// <summary>
    /// Force set animation speed (for matching movement)
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            animator.speed = speed;
        }
    }

    /// <summary>
    /// Reset animation speed to normal
    /// </summary>
    public void ResetAnimationSpeed()
    {
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Play Scream")]
    public void TestPlayScream()
    {
        PlayScream();
    }

    [ContextMenu("Test: Play Search")]
    public void TestPlaySearch()
    {
        PlaySearch();
    }

    [ContextMenu("Test: Play Attack")]
    public void TestPlayAttack()
    {
        PlayAttack();
    }
}
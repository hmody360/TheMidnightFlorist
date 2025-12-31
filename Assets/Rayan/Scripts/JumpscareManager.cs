using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

// ================================================================================
// JUMPSCARE MANAGER
// ================================================================================
// Handles the jumpscare sequence when monster catches the player.
// 
// SETUP INSTRUCTIONS:
// 1. Create an empty GameObject named "JumpscareManager"
// 2. Add this script to it
// 3. Create a UI Image for the black fade (full screen, black color, alpha = 0)
// 4. Assign references in Inspector
// 5. Add "Jumpscare" trigger to Monster's Animator Controller
//
// ANIMATOR SETUP:
// 1. Add parameter: "Jumpscare" (Trigger)
// 2. Create transition: Any State -> Jumpscare Animation
//    - Condition: Jumpscare trigger
//    - Uncheck Has Exit Time
//
// WORKS WITH: Cinemachine camera system
// ================================================================================

public class JumpscareManager : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static JumpscareManager Instance { get; private set; }

    // ==================== REFERENCES ====================
    [Header("=== REFERENCES ===")]
    [Tooltip("Main Camera transform (auto-found if not set)")]
    public Transform mainCamera;

    [Tooltip("Cinemachine Virtual Camera (auto-found if not set)")]
    public GameObject cinemachineVirtualCamera;

    [Tooltip("Player movement script (auto-found if not set)")]
    public PlayerMovement playerMovement;

    [Tooltip("Monster transform (auto-found if not set)")]
    public Transform monster;

    [Tooltip("Monster's Animator (auto-found from monster if not set)")]
    public Animator monsterAnimator;

    [Tooltip("Monster's NavMeshAgent (auto-found from monster if not set)")]
    public UnityEngine.AI.NavMeshAgent monsterNavAgent;

    // ==================== JUMPSCARE SETTINGS ====================
    [Header("=== JUMPSCARE POSITIONING ===")]
    [Tooltip("Distance between monster and player for jumpscare animation")]
    public float jumpscareDistance = 2f;

    [Tooltip("Height offset for monster during jumpscare (0 = ground level)")]
    public float monsterHeightOffset = 0f;

    [Header("=== CAMERA SETTINGS ===")]
    [Tooltip("How fast camera snaps to look at monster (seconds)")]
    public float cameraSnapDuration = 0.2f;

    [Tooltip("If true, camera will move to monster's face position during jumpscare (recommended for big monsters)")]
    public bool attachCameraToMonsterFace = true;

    [Tooltip("Distance from monster's face to camera (0.3 = very close, 1.0 = further back)")]
    public float cameraFaceOffset = 0.5f;

    [Header("=== TIMING ===")]
    [Tooltip("Duration of jumpscare animation")]
    public float jumpscareAnimationDuration = 1.5f;

    [Tooltip("Delay before starting fade to black (should be >= animation duration!)")]
    public float fadeDelay = 1.5f;

    [Tooltip("Duration of fade to black")]
    public float fadeDuration = 0.5f;

    [Tooltip("Delay after fade before showing game over")]
    public float gameOverDelay = 0.5f;

    // ==================== AUDIO ====================
    [Header("=== AUDIO ===")]
    [Tooltip("Jumpscare sound effect")]
    public AudioClip jumpscareSound;

    [Tooltip("Jumpscare sound volume")]
    [Range(0f, 1f)]
    public float jumpscareVolume = 1f;

    // ==================== UI ====================
    [Header("=== UI EFFECTS ===")]
    [Tooltip("Full screen black image for fade effect (create in Canvas)")]
    public Image blackFadeImage;

    [Tooltip("Full screen white image for flash effect (create in Canvas, optional)")]
    public Image whiteFlashImage;

    // ==================== FLASH LIGHT ====================
    [Header("=== MONSTER FACE LIGHT ===")]
    [Tooltip("Spotlight to illuminate monster's face during jumpscare (CREATE: Right-click Main Camera ? Light ? Spot Light)")]
    public Light monsterFaceLight;

    [Tooltip("Intensity of the light on monster's face")]
    public float faceLightIntensity = 5f;

    [Tooltip("Range of the spotlight")]
    public float faceLightRange = 10f;

    [Tooltip("Spot angle of the light")]
    public float faceLightAngle = 60f;

    [Header("=== WHITE FLASH (Screen Effect) ===")]
    [Tooltip("Duration of white flash at start of jumpscare")]
    public float flashDuration = 0.1f;

    [Header("=== FACE TRACKING ===")]
    [Tooltip("If monster has a face/head bone, assign it here (Head or Neck bone recommended, not Jaw)")]
    public Transform monsterFaceBone;

    [Tooltip("Offset FROM the bone position (use Y to look higher than the bone). Works with bone assigned.")]
    public Vector3 boneOffset = new Vector3(0f, 0.5f, 0f);

    [Tooltip("If NO face bone assigned, use this offset from monster's ROOT position")]
    public Vector3 faceOffsetFromRoot = new Vector3(0f, 3f, 0f);

    [Header("=== CAMERA ROTATION OVERRIDE ===")]
    [Tooltip("If true, use manual rotation instead of looking at face automatically")]
    public bool useManualCameraRotation = false;

    [Tooltip("Manual camera rotation during jumpscare (Euler angles). Only used if above is checked.")]
    public Vector3 manualCameraRotation = new Vector3(0f, 180f, 0f);

    // ==================== POST PROCESSING (BRIGHTNESS) ====================
    [Header("=== POST PROCESSING (Brighten Scene) ===")]
    [Tooltip("Global Volume used for post processing (auto-found if not set)")]
    public Volume globalVolume;

    [Tooltip("Enable post processing brightness change during jumpscare")]
    public bool useBrightnessChange = true;

    [Tooltip("Post Exposure during jumpscare. Your normal is -0.8, so try 0 or 0.5 for subtle brightening")]
    public float jumpscareExposure = 0.5f;

    [Tooltip("Should we reduce vignette during jumpscare?")]
    public bool reduceVignette = true;

    // ==================== ANIMATOR PARAMETER ====================
    [Header("=== ANIMATOR ===")]
    [Tooltip("Trigger name for jumpscare animation")]
    public string jumpscareTrigger = "Jumpscare";

    // ==================== DEBUG ====================
    [Header("=== DEBUG ===")]
    public bool showDebugLogs = true;

    // ==================== PRIVATE VARIABLES ====================
    private AudioSource audioSource;
    private bool isJumpscareActive = false;
    private Coroutine cameraLockCoroutine;
    private float originalLightIntensity = 0f;
    private float originalLightRange = 0f;
    private float originalLightAngle = 0f;
    private bool lightWasEnabled = false;

    // Post Processing
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    private float originalExposure = 0f;
    private float originalVignetteIntensity = 0f;

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
            Debug.LogWarning("JumpscareManager: Multiple instances detected!");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Setup audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for jumpscare

        // Auto-find references
        FindReferences();

        // Make sure fade image starts transparent
        if (blackFadeImage != null)
        {
            Color c = blackFadeImage.color;
            c.a = 0f;
            blackFadeImage.color = c;
            blackFadeImage.gameObject.SetActive(false);
        }

        if (showDebugLogs)
        {
            Debug.Log("JumpscareManager: Initialized");
        }
    }

    private void FindReferences()
    {
        // Find main camera
        if (mainCamera == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                mainCamera = cam.transform;
            }
        }

        // Find Cinemachine Virtual Camera (FirstPersonCamera)
        if (cinemachineVirtualCamera == null)
        {
            // Try to find by name
            GameObject vcam = GameObject.Find("FirstPersonCamera");
            if (vcam != null)
            {
                cinemachineVirtualCamera = vcam;
            }
        }

        // Find player movement
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        // Find monster
        if (monster == null)
        {
            MonsterAI monsterAI = MonsterAI.Instance;
            if (monsterAI != null)
            {
                monster = monsterAI.transform;
            }
        }

        // Find monster components
        if (monster != null)
        {
            if (monsterAnimator == null)
            {
                monsterAnimator = monster.GetComponent<Animator>();
                if (monsterAnimator == null)
                {
                    monsterAnimator = monster.GetComponentInChildren<Animator>();
                }
            }

            if (monsterNavAgent == null)
            {
                monsterNavAgent = monster.GetComponent<UnityEngine.AI.NavMeshAgent>();
            }
        }

        // Find Global Volume for post processing
        if (globalVolume == null)
        {
            // Try to find by name first
            GameObject volumeObj = GameObject.Find("Global Volume");
            if (volumeObj != null)
            {
                globalVolume = volumeObj.GetComponent<Volume>();
            }

            // If not found, search for any Volume with global mode
            if (globalVolume == null)
            {
                Volume[] volumes = FindObjectsOfType<Volume>();
                foreach (Volume vol in volumes)
                {
                    if (vol.isGlobal)
                    {
                        globalVolume = vol;
                        break;
                    }
                }
            }
        }

        // Get post processing components from volume
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out colorAdjustments);
            globalVolume.profile.TryGet(out vignette);

            if (showDebugLogs)
            {
                Debug.Log($"JumpscareManager: Found Global Volume. ColorAdjustments: {colorAdjustments != null}, Vignette: {vignette != null}");
            }
        }

        // Validation
        if (mainCamera == null)
            Debug.LogError("JumpscareManager: Main camera not found!");
        if (monster == null)
            Debug.LogError("JumpscareManager: Monster not found!");
        if (blackFadeImage == null)
            Debug.LogWarning("JumpscareManager: Black fade image not assigned! Fade effect won't work.");
        if (globalVolume == null)
            Debug.LogWarning("JumpscareManager: Global Volume not found! Scene won't brighten during jumpscare.");
    }

    // ==================== PUBLIC METHODS ====================

    /// <summary>
    /// Trigger the jumpscare sequence. Called by MonsterAI when attack hits.
    /// </summary>
    public void TriggerJumpscare()
    {
        if (isJumpscareActive)
        {
            if (showDebugLogs) Debug.Log("JumpscareManager: Jumpscare already active, ignoring");
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log("JumpscareManager: JUMPSCARE TRIGGERED!");
        }

        StartCoroutine(JumpscareSequence());
    }

    // ==================== JUMPSCARE SEQUENCE ====================

    private IEnumerator JumpscareSequence()
    {
        isJumpscareActive = true;

        // ===== STEP 1: Disable player controls =====
        DisablePlayerControls();

        // ===== STEP 2: Stop monster movement =====
        StopMonsterMovement();

        // ===== STEP 3: Position monster in front of player =====
        PositionMonsterForJumpscare();

        // ===== STEP 4: BRIGHTEN THE SCENE (Post Processing) =====
        BrightenScene();

        // ===== STEP 5: Turn on spotlight on monster face =====
        TurnOnJumpscareLight();

        // ===== STEP 6: White flash effect =====
        yield return StartCoroutine(WhiteFlash());

        // ===== STEP 7: Snap camera to look at monster =====
        yield return StartCoroutine(SnapCameraToMonster());

        // ===== STEP 8: Play jumpscare animation =====
        PlayJumpscareAnimation();

        // ===== STEP 9: Play jumpscare sound =====
        PlayJumpscareSound();

        // ===== STEP 10: Keep camera locked on monster =====
        cameraLockCoroutine = StartCoroutine(KeepCameraLockedOnMonster());

        // ===== STEP 11: Wait for animation, then fade =====
        yield return new WaitForSeconds(fadeDelay);

        // ===== STEP 12: Turn off light before fade =====
        TurnOffJumpscareLight();

        // ===== STEP 13: Fade to black =====
        yield return StartCoroutine(FadeToBlack());

        // ===== STEP 14: Restore scene brightness (before game over screen) =====
        RestoreSceneBrightness();

        // ===== STEP 15: Stop camera lock =====
        if (cameraLockCoroutine != null)
        {
            StopCoroutine(cameraLockCoroutine);
        }

        // ===== STEP 16: Wait a moment =====
        yield return new WaitForSeconds(gameOverDelay);

        // ===== STEP 17: Trigger Game Over =====
        TriggerGameOver();
    }

    // ==================== STEP METHODS ====================

    private void DisablePlayerControls()
    {
        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }

        // Disable Cinemachine Virtual Camera (so it stops controlling the main camera)
        if (cinemachineVirtualCamera != null)
        {
            cinemachineVirtualCamera.SetActive(false);

            if (showDebugLogs)
            {
                Debug.Log("JumpscareManager: Cinemachine Virtual Camera disabled");
            }
        }

        if (showDebugLogs)
        {
            Debug.Log("JumpscareManager: Player controls disabled");
        }
    }

    private void StopMonsterMovement()
    {
        if (monsterNavAgent != null)
        {
            // Only stop if agent is active and on NavMesh
            if (monsterNavAgent.isActiveAndEnabled && monsterNavAgent.isOnNavMesh)
            {
                monsterNavAgent.isStopped = true;
                monsterNavAgent.velocity = Vector3.zero;
                monsterNavAgent.speed = 0f;
            }

            // Disable the agent to allow teleporting
            monsterNavAgent.enabled = false;
        }

        if (showDebugLogs)
        {
            Debug.Log("JumpscareManager: Monster movement stopped");
        }
    }

    private void PositionMonsterForJumpscare()
    {
        if (monster == null || mainCamera == null) return;

        // Get horizontal direction from camera (ignore vertical)
        Vector3 cameraForward = mainCamera.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        // Position monster in front of player at specified distance
        Vector3 jumpscarePosition = mainCamera.position + cameraForward * jumpscareDistance;

        // Set monster to ground level (or with offset)
        jumpscarePosition.y = monster.position.y + monsterHeightOffset;

        // NavMeshAgent is already disabled in StopMonsterMovement()

        // Teleport monster
        monster.position = jumpscarePosition;

        // Make monster face the player
        Vector3 lookAtPosition = new Vector3(mainCamera.position.x, monster.position.y, mainCamera.position.z);
        monster.LookAt(lookAtPosition);

        if (showDebugLogs)
        {
            Debug.Log($"JumpscareManager: Monster positioned at {jumpscarePosition}, distance: {jumpscareDistance}");
        }
    }

    private IEnumerator SnapCameraToMonster()
    {
        if (mainCamera == null || monster == null) yield break;

        float elapsed = 0f;
        Quaternion startRotation = mainCamera.rotation;
        Vector3 startPosition = mainCamera.position;

        // Get monster face position
        Vector3 monsterFacePosition = GetMonsterFacePosition();

        // Calculate target camera position (in front of monster's face)
        Vector3 targetCameraPosition = startPosition;
        if (attachCameraToMonsterFace)
        {
            // Position camera in front of monster's face
            Vector3 directionFromFace = (startPosition - monsterFacePosition).normalized;
            targetCameraPosition = monsterFacePosition + directionFromFace * cameraFaceOffset;
        }

        while (elapsed < cameraSnapDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cameraSnapDuration;

            // Update face position (in case monster moved)
            monsterFacePosition = GetMonsterFacePosition();

            // Smoothly move camera if attach mode is on
            if (attachCameraToMonsterFace)
            {
                Vector3 directionFromFace = (startPosition - monsterFacePosition).normalized;
                Vector3 currentTarget = monsterFacePosition + directionFromFace * cameraFaceOffset;
                mainCamera.position = Vector3.Lerp(startPosition, currentTarget, t);
            }

            // Calculate target rotation
            Quaternion targetRotation;
            if (useManualCameraRotation)
            {
                // Use manual rotation
                targetRotation = Quaternion.Euler(manualCameraRotation);
            }
            else
            {
                // Auto look at monster face
                Vector3 directionToFace = monsterFacePosition - mainCamera.position;
                targetRotation = Quaternion.LookRotation(directionToFace);
            }

            // Smoothly rotate camera
            mainCamera.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        // Final position and rotation
        monsterFacePosition = GetMonsterFacePosition();
        if (attachCameraToMonsterFace)
        {
            Vector3 directionFromFace = (mainCamera.position - monsterFacePosition).normalized;
            mainCamera.position = monsterFacePosition + directionFromFace * cameraFaceOffset;
        }

        // Final rotation
        if (useManualCameraRotation)
        {
            mainCamera.rotation = Quaternion.Euler(manualCameraRotation);
        }
        else
        {
            Vector3 finalDirection = monsterFacePosition - mainCamera.position;
            if (finalDirection != Vector3.zero)
            {
                mainCamera.rotation = Quaternion.LookRotation(finalDirection);
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"JumpscareManager: Camera snapped to monster face");
        }
    }

    private IEnumerator KeepCameraLockedOnMonster()
    {
        if (mainCamera == null || monster == null) yield break;

        // Store initial direction from face to camera (to maintain consistent angle)
        Vector3 initialFacePos = GetMonsterFacePosition();
        Vector3 initialDirection = (mainCamera.position - initialFacePos).normalized;

        while (true)
        {
            // Get current monster face position (follows animation!)
            Vector3 monsterFacePosition = GetMonsterFacePosition();

            // If attach mode, keep camera in front of face (follows the animation)
            if (attachCameraToMonsterFace)
            {
                mainCamera.position = monsterFacePosition + initialDirection * cameraFaceOffset;
            }

            // Set camera rotation
            if (useManualCameraRotation)
            {
                // Use manual rotation
                mainCamera.rotation = Quaternion.Euler(manualCameraRotation);
            }
            else
            {
                // Keep camera looking at monster face
                Vector3 directionToFace = monsterFacePosition - mainCamera.position;
                if (directionToFace != Vector3.zero)
                {
                    mainCamera.rotation = Quaternion.LookRotation(directionToFace);
                }
            }

            // Also update light to point at face
            PointLightAtMonsterFace();

            yield return null;
        }
    }

    private void PlayJumpscareAnimation()
    {
        if (monsterAnimator == null) return;

        monsterAnimator.SetTrigger(jumpscareTrigger);

        if (showDebugLogs)
        {
            Debug.Log("JumpscareManager: Jumpscare animation triggered");
        }
    }

    private void PlayJumpscareSound()
    {
        if (jumpscareSound == null)
        {
            if (showDebugLogs) Debug.Log("JumpscareManager: No jumpscare sound assigned");
            return;
        }

        audioSource.PlayOneShot(jumpscareSound, jumpscareVolume);

        if (showDebugLogs)
        {
            Debug.Log("JumpscareManager: Jumpscare sound playing");
        }
    }

    // ==================== FLASH & LIGHT EFFECTS ====================

    private void TurnOnJumpscareLight()
    {
        if (monsterFaceLight != null)
        {
            // Save original values
            originalLightIntensity = monsterFaceLight.intensity;
            originalLightRange = monsterFaceLight.range;
            originalLightAngle = monsterFaceLight.spotAngle;
            lightWasEnabled = monsterFaceLight.enabled;

            // Set spotlight settings
            monsterFaceLight.type = LightType.Spot;
            monsterFaceLight.intensity = faceLightIntensity;
            monsterFaceLight.range = faceLightRange;
            monsterFaceLight.spotAngle = faceLightAngle;
            monsterFaceLight.enabled = true;

            // Point light at monster face
            PointLightAtMonsterFace();

            if (showDebugLogs)
            {
                Debug.Log("JumpscareManager: Monster face light ON");
            }
        }
    }

    private void TurnOffJumpscareLight()
    {
        if (monsterFaceLight != null)
        {
            // Restore original values
            monsterFaceLight.intensity = originalLightIntensity;
            monsterFaceLight.range = originalLightRange;
            monsterFaceLight.spotAngle = originalLightAngle;
            monsterFaceLight.enabled = lightWasEnabled;

            if (showDebugLogs)
            {
                Debug.Log("JumpscareManager: Monster face light OFF");
            }
        }
    }

    private void PointLightAtMonsterFace()
    {
        if (monsterFaceLight == null) return;

        Vector3 facePosition = GetMonsterFacePosition();

        // Point the light at the face
        Vector3 directionToFace = facePosition - monsterFaceLight.transform.position;
        if (directionToFace != Vector3.zero)
        {
            monsterFaceLight.transform.rotation = Quaternion.LookRotation(directionToFace);
        }
    }

    /// <summary>
    /// Get monster face position - uses face bone + offset if assigned, otherwise uses root offset
    /// </summary>
    private Vector3 GetMonsterFacePosition()
    {
        if (monsterFaceBone != null)
        {
            // Use the bone position + offset (offset is in world space Y)
            return monsterFaceBone.position + boneOffset;
        }
        else if (monster != null)
        {
            // Use monster root position + offset
            return monster.position + monster.TransformDirection(faceOffsetFromRoot);
        }
        return Vector3.zero;
    }

    // ==================== POST PROCESSING CONTROL ====================

    /// <summary>
    /// Brighten the scene by adjusting post processing
    /// </summary>
    private void BrightenScene()
    {
        if (!useBrightnessChange)
        {
            if (showDebugLogs) Debug.Log("JumpscareManager: Brightness change disabled, using light only");
            return;
        }

        if (colorAdjustments != null)
        {
            // Save original exposure
            originalExposure = colorAdjustments.postExposure.value;

            // Set bright exposure for jumpscare
            colorAdjustments.postExposure.value = jumpscareExposure;

            if (showDebugLogs)
            {
                Debug.Log($"JumpscareManager: Scene brightened (Exposure: {originalExposure} -> {jumpscareExposure})");
            }
        }

        if (reduceVignette && vignette != null)
        {
            // Save original vignette
            originalVignetteIntensity = vignette.intensity.value;

            // Reduce vignette during jumpscare
            vignette.intensity.value = 0f;

            if (showDebugLogs)
            {
                Debug.Log($"JumpscareManager: Vignette reduced (Intensity: {originalVignetteIntensity} -> 0)");
            }
        }
    }

    /// <summary>
    /// Restore original post processing settings
    /// </summary>
    private void RestoreSceneBrightness()
    {
        if (!useBrightnessChange) return;

        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = originalExposure;

            if (showDebugLogs)
            {
                Debug.Log($"JumpscareManager: Scene brightness restored (Exposure: {originalExposure})");
            }
        }

        if (reduceVignette && vignette != null)
        {
            vignette.intensity.value = originalVignetteIntensity;

            if (showDebugLogs)
            {
                Debug.Log($"JumpscareManager: Vignette restored (Intensity: {originalVignetteIntensity})");
            }
        }
    }

    // ==================== WHITE FLASH ====================

    private IEnumerator WhiteFlash()
    {
        if (whiteFlashImage == null)
        {
            if (showDebugLogs) Debug.Log("JumpscareManager: No white flash image, skipping flash");
            yield break;
        }

        // Enable and show white flash
        whiteFlashImage.gameObject.SetActive(true);
        Color color = whiteFlashImage.color;
        color.a = 1f;
        whiteFlashImage.color = color;

        if (showDebugLogs)
        {
            Debug.Log("JumpscareManager: White flash!");
        }

        // Hold flash briefly
        yield return new WaitForSeconds(flashDuration * 0.3f);

        // Fade out flash
        float elapsed = 0f;
        float fadeOutDuration = flashDuration * 0.7f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;

            color.a = Mathf.Lerp(1f, 0f, t);
            whiteFlashImage.color = color;

            yield return null;
        }

        // Hide flash image
        color.a = 0f;
        whiteFlashImage.color = color;
        whiteFlashImage.gameObject.SetActive(false);
    }

    // ==================== FADE TO BLACK ====================

    private IEnumerator FadeToBlack()
    {
        if (blackFadeImage == null)
        {
            if (showDebugLogs) Debug.Log("JumpscareManager: No fade image, skipping fade");
            yield break;
        }

        // Enable the fade image
        blackFadeImage.gameObject.SetActive(true);

        float elapsed = 0f;
        Color color = blackFadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Fade from transparent to black
            color.a = Mathf.Lerp(0f, 1f, t);
            blackFadeImage.color = color;

            yield return null;
        }

        // Ensure fully black
        color.a = 1f;
        blackFadeImage.color = color;

        if (showDebugLogs)
        {
            Debug.Log("JumpscareManager: Fade to black complete");
        }
    }

    private void TriggerGameOver()
    {
        if (showDebugLogs)
        {
            Debug.Log("JumpscareManager: Triggering Game Over");
        }

        // Reset time scale if changed
        Time.timeScale = 1f;

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Call NightGameManager to handle game over
        if (NightGameManager.Instance != null)
        {
            NightGameManager.Instance.OnPlayerCaughtByMonster();
        }
        else
        {
            // Fallback: Try to find GameManager by name
            GameObject gameManager = GameObject.Find("GameManager");
            if (gameManager != null)
            {
                gameManager.SendMessage("OnPlayerCaughtByMonster", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning("JumpscareManager: NightGameManager not found! Cannot trigger game over.");
            }
        }
    }

    // ==================== PUBLIC UTILITY METHODS ====================

    /// <summary>
    /// Reset jumpscare state (call when restarting game)
    /// </summary>
    public void ResetJumpscare()
    {
        isJumpscareActive = false;

        // Reset fade image
        if (blackFadeImage != null)
        {
            Color c = blackFadeImage.color;
            c.a = 0f;
            blackFadeImage.color = c;
            blackFadeImage.gameObject.SetActive(false);
        }

        // Stop any running coroutines
        StopAllCoroutines();

        if (showDebugLogs)
        {
            Debug.Log("JumpscareManager: Reset complete");
        }
    }

    /// <summary>
    /// Check if jumpscare is currently active
    /// </summary>
    public bool IsJumpscareActive()
    {
        return isJumpscareActive;
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Trigger Jumpscare")]
    public void TestTriggerJumpscare()
    {
        TriggerJumpscare();
    }

    [ContextMenu("Test: Reset Jumpscare")]
    public void TestResetJumpscare()
    {
        ResetJumpscare();
    }

    [ContextMenu("Test: Fade To Black Only")]
    public void TestFadeToBlack()
    {
        StartCoroutine(FadeToBlack());
    }
}
using UnityEngine;

// ================================================================================
// FLOWER INDICATOR - Ring that shows above flowers
// ================================================================================
// Shows a rotating ring above flowers when player is in range.
// 
// SETUP:
// 1. Add this script to the SAME GameObject as FlowerPickup
// 2. Adjust the Ring Offset to center the ring above your flower model
// 3. Set blocking layers for visibility check (walls, obstacles)
//
// NOTE: The ring is NOT a child of the flower (to avoid rotation/scale issues)
//       but it will automatically destroy itself when the flower is destroyed.
//
// IMPORTANT: Ring offset is in WORLD space directions:
//   - Y = height above flower (world up)
//   - X/Z = horizontal offset (world directions)
// ================================================================================

public class FlowerIndicator : MonoBehaviour
{
    // ==================== DISTANCE SETTINGS ====================
    [Header("=== DISTANCE SETTINGS ===")]
    [Tooltip("Minimum distance to show ring (closer = show prompt instead)")]
    public float minDistance = 2f;

    [Tooltip("Maximum distance to show ring (further = hidden)")]
    public float maxDistance = 5f;

    // ==================== RAYCAST SETTINGS ====================
    [Header("=== VISIBILITY CHECK ===")]
    [Tooltip("Layers that block visibility (walls, bushes, etc.)")]
    public LayerMask blockingLayers;

    [Tooltip("How often to check visibility (seconds)")]
    public float checkInterval = 0.1f;

    // ==================== RING POSITION ====================
    [Header("=== RING POSITION OFFSET (WORLD SPACE) ===")]
    [Tooltip("Offset from flower center in WORLD directions (Y = up, not affected by flower rotation)")]
    public Vector3 ringOffset = new Vector3(0f, 0.5f, 0f);

    // ==================== RING SETTINGS ====================
    [Header("=== RING APPEARANCE ===")]
    [Tooltip("Ring color (alpha will be controlled by distance)")]
    public Color ringColor = Color.white;

    [Tooltip("Ring radius")]
    public float ringRadius = 0.8f;

    [Tooltip("Ring line thickness")]
    public float ringThickness = 0.05f;

    [Tooltip("Number of segments (more = smoother circle)")]
    public int segments = 32;

    [Header("=== RING ANIMATION ===")]
    [Tooltip("Rotation speed (degrees per second)")]
    public float rotationSpeed = 30f;

    [Tooltip("Fade IN speed (higher = faster fade in)")]
    public float fadeInSpeed = 2f;

    [Tooltip("Fade OUT speed (higher = faster fade out)")]
    public float fadeOutSpeed = 3f;

    [Tooltip("Bob up and down")]
    public bool enableBobbing = true;

    [Tooltip("Bob speed")]
    public float bobSpeed = 2f;

    [Tooltip("Bob amount")]
    public float bobAmount = 0.1f;

    [Header("=== DEBUG ===")]
    [Tooltip("Show debug messages")]
    public bool showDebugLogs = false;

    // ==================== PRIVATE VARIABLES ====================
    private Transform playerTransform;
    private LineRenderer lineRenderer;
    private GameObject ringObject;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;
    private float checkTimer = 0f;
    private float bobTimer = 0f;
    private float currentRotationY = 0f; // Track rotation separately for smooth spinning
    private bool isVisible = false;
    private bool isInitialized = false;

    // ==================== UNITY METHODS ====================
    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            if (showDebugLogs) Debug.Log("FlowerIndicator: Player found!");
        }
        else
        {
            Debug.LogWarning("FlowerIndicator: Player not found! Make sure player has 'Player' tag.");
        }

        // Create the ring
        CreateRing();

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // Always update animation and fade
        UpdateRingAnimation();
        UpdateFade();

        // Only check visibility if player exists
        if (playerTransform == null)
        {
            targetAlpha = 0f;
            return;
        }

        // Check visibility periodically (for performance)
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckVisibility();
        }
    }

    void OnDestroy()
    {
        // IMPORTANT: Ring is NOT a child, so we must manually destroy it
        if (ringObject != null)
        {
            Destroy(ringObject);
            ringObject = null;
        }
        lineRenderer = null;

        if (showDebugLogs) Debug.Log("FlowerIndicator: Flower destroyed, ring cleaned up.");
    }

    // ==================== RING CREATION ====================
    private void CreateRing()
    {
        // ============================================================
        // IMPORTANT: Ring is NOT a child of the flower!
        // This prevents rotation/scale inheritance issues.
        // We manually track position and destroy when flower is destroyed.
        // ============================================================

        ringObject = new GameObject("FlowerRing_" + gameObject.GetInstanceID());
        // DO NOT parent: ringObject.transform.SetParent(transform);

        // Set initial position in WORLD space (flower position + offset)
        ringObject.transform.position = transform.position + ringOffset;

        // Set rotation to flat (world space) - always horizontal
        ringObject.transform.rotation = Quaternion.identity;

        // Set scale to 1,1,1 (no scaling)
        ringObject.transform.localScale = Vector3.one;

        // Add line renderer
        lineRenderer = ringObject.AddComponent<LineRenderer>();

        // Configure line renderer
        // Using world space = false means positions are relative to ringObject
        // Since ringObject has identity rotation and scale, this works correctly
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = ringThickness;
        lineRenderer.endWidth = ringThickness;

        // Create a material that supports transparency
        Material mat = new Material(Shader.Find("Sprites/Default"));
        if (mat == null || mat.shader == null)
        {
            mat = new Material(Shader.Find("UI/Default"));
        }
        if (mat != null)
        {
            lineRenderer.material = mat;
        }

        // Set ring positions (circle shape) - flat on XZ plane (local to ring)
        Vector3[] positions = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * ringRadius;
            float z = Mathf.Sin(angle) * ringRadius;
            positions[i] = new Vector3(x, 0f, z);
        }
        lineRenderer.SetPositions(positions);

        // Start invisible (alpha = 0)
        currentAlpha = 0f;
        targetAlpha = 0f;
        currentRotationY = 0f;
        SetRingAlpha(0f);

        if (showDebugLogs) Debug.Log("FlowerIndicator: Ring created (independent object) for " + gameObject.name);
    }

    // ==================== VISIBILITY CHECK ====================
    private void CheckVisibility()
    {
        if (playerTransform == null)
        {
            targetAlpha = 0f;
            isVisible = false;
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (showDebugLogs) Debug.Log($"FlowerIndicator: Distance = {distance:F1}, min={minDistance}, max={maxDistance}, targetAlpha={targetAlpha}, currentAlpha={currentAlpha:F2}");

        // Check if player is too close
        if (distance <= minDistance)
        {
            if (showDebugLogs) Debug.Log("FlowerIndicator: Too close - hiding");
            targetAlpha = 0f;
            isVisible = false;
            return;
        }

        // Check if player is too far
        if (distance > maxDistance)
        {
            if (showDebugLogs) Debug.Log("FlowerIndicator: Too far - hiding");
            targetAlpha = 0f;
            isVisible = false;
            return;
        }

        // Player is in range - check line of sight
        if (HasLineOfSight())
        {
            if (showDebugLogs) Debug.Log("FlowerIndicator: In range - showing");
            targetAlpha = 1f;
            isVisible = true;
        }
        else
        {
            if (showDebugLogs) Debug.Log("FlowerIndicator: Blocked - hiding");
            targetAlpha = 0f;
            isVisible = false;
        }
    }

    private bool HasLineOfSight()
    {
        if (playerTransform == null) return false;

        // If no blocking layers set, always return true
        if (blockingLayers == 0)
        {
            return true;
        }

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distance = directionToPlayer.magnitude;

        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, distance, blockingLayers))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    // ==================== RING ANIMATION ====================
    private void UpdateRingAnimation()
    {
        if (ringObject == null) return;

        // ============================================================
        // POSITION: Follow flower in WORLD space
        // ringOffset.Y = world up (not affected by flower rotation)
        // ============================================================

        // Calculate bobbing offset (world up direction)
        float bobOffsetAmount = 0f;
        if (enableBobbing)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            bobOffsetAmount = Mathf.Sin(bobTimer) * bobAmount;
        }

        // Set ring position: flower position + offset + bob (all in world space)
        Vector3 targetWorldPosition = transform.position + ringOffset + Vector3.up * bobOffsetAmount;
        ringObject.transform.position = targetWorldPosition;

        // ============================================================
        // ROTATION: Always flat, only spin around Y axis
        // ============================================================

        // Update rotation (spin around world Y axis)
        currentRotationY += rotationSpeed * Time.deltaTime;
        if (currentRotationY >= 360f) currentRotationY -= 360f;

        // Apply flat rotation (only Y rotation for spinning, X and Z are 0)
        ringObject.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);

        // ============================================================
        // SCALE: Always 1,1,1 (no scaling from parent)
        // ============================================================
        ringObject.transform.localScale = Vector3.one;
    }

    private void UpdateFade()
    {
        if (lineRenderer == null) return;

        // Choose fade speed based on direction
        float speed = (targetAlpha > currentAlpha) ? fadeInSpeed : fadeOutSpeed;

        // Smoothly fade to target alpha
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, speed * Time.deltaTime);

        // Apply the alpha
        SetRingAlpha(currentAlpha);
    }

    private void SetRingAlpha(float alpha)
    {
        if (lineRenderer == null) return;

        // Create color with current alpha
        Color color = new Color(ringColor.r, ringColor.g, ringColor.b, alpha);

        // Set start and end colors on LineRenderer
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"FlowerIndicator: SetRingAlpha({alpha:F2}), color={color}");
        }
    }

    // ==================== PUBLIC METHODS ====================
    public bool IsRingVisible()
    {
        return isVisible;
    }

    public void ForceHide()
    {
        targetAlpha = 0f;
        isVisible = false;
    }

    public void SetRingColor(Color newColor)
    {
        ringColor = newColor;
        SetRingAlpha(currentAlpha);
    }

    public void SetRingOffset(Vector3 newOffset)
    {
        ringOffset = newOffset;
        // Position will be updated in next UpdateRingAnimation call
    }

    // ==================== EDITOR GIZMOS ====================
    void OnDrawGizmos()
    {
        // Draw ring preview in editor (world space position)
        Gizmos.color = ringColor;
        Vector3 ringPosition = transform.position + ringOffset;

        // Draw circle
        int previewSegments = 16;
        Vector3 previousPoint = ringPosition + new Vector3(ringRadius, 0f, 0f);
        for (int i = 1; i <= previewSegments; i++)
        {
            float angle = (float)i / previewSegments * 360f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * ringRadius;
            float z = Mathf.Sin(angle) * ringRadius;
            Vector3 currentPoint = ringPosition + new Vector3(x, 0f, z);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        // Draw line from flower to ring position
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, ringPosition);
    }

    void OnDrawGizmosSelected()
    {
        // Draw distance spheres
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        // Draw ring center point
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + ringOffset, 0.1f);
    }
}
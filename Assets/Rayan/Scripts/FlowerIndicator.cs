using UnityEngine;


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

    // ==================== RING SETTINGS ====================
    [Header("=== RING APPEARANCE ===")]
    [Tooltip("Ring color")]
    public Color ringColor = Color.white;

    [Tooltip("Ring radius")]
    public float ringRadius = 0.8f;

    [Tooltip("Ring line thickness")]
    public float ringThickness = 0.05f;

    [Tooltip("Height offset above flower")]
    public float heightOffset = 0.5f;

    [Tooltip("Number of segments (more = smoother circle)")]
    public int segments = 32;

    [Header("=== RING ANIMATION ===")]
    [Tooltip("Rotation speed (degrees per second)")]
    public float rotationSpeed = 30f;

    [Tooltip("Fade in/out speed")]
    public float fadeSpeed = 5f;

    [Tooltip("Bob up and down")]
    public bool enableBobbing = true;

    [Tooltip("Bob speed")]
    public float bobSpeed = 2f;

    [Tooltip("Bob amount")]
    public float bobAmount = 0.1f;

    // ==================== PRIVATE VARIABLES ====================
    private Transform playerTransform;
    private LineRenderer lineRenderer;
    private GameObject ringObject;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;
    private float checkTimer = 0f;
    private float bobTimer = 0f;
    private Vector3 basePosition;
    private bool isVisible = false;
    private bool playerInPromptRange = false;

    // ==================== UNITY METHODS ====================
    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("FlowerIndicator: Player not found! Make sure player has 'Player' tag.");
        }

        // Create the ring
        CreateRing();

        // Store base position
        basePosition = transform.position + Vector3.up * heightOffset;
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Check visibility periodically (for performance)
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckVisibility();
        }

        // Update ring animation
        UpdateRingAnimation();

        // Update fade
        UpdateFade();
    }

    void OnDestroy()
    {
        // Destroy ring object
        if (ringObject != null)
        {
            Destroy(ringObject);
        }
    }

    // ==================== RING CREATION ====================
    private void CreateRing()
    {
        // Create ring game object
        ringObject = new GameObject("FlowerRing");
        ringObject.transform.SetParent(transform);
        ringObject.transform.localPosition = Vector3.up * heightOffset;

        // Add line renderer
        lineRenderer = ringObject.AddComponent<LineRenderer>();

        // Configure line renderer
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = ringThickness;
        lineRenderer.endWidth = ringThickness;

        // Create material
        Material ringMaterial = new Material(Shader.Find("Sprites/Default"));
        ringMaterial.color = ringColor;
        lineRenderer.material = ringMaterial;

        // Set ring positions (circle shape)
        Vector3[] positions = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * ringRadius;
            float z = Mathf.Sin(angle) * ringRadius;
            positions[i] = new Vector3(x, 0f, z);
        }
        lineRenderer.SetPositions(positions);

        // Start invisible
        SetRingAlpha(0f);
    }

    // ==================== VISIBILITY CHECK ====================
    private void CheckVisibility()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Check if player is in prompt range (close)
        if (distance <= minDistance)
        {
            // Too close - hide ring, show prompt handled by RayInteractor
            targetAlpha = 0f;
            isVisible = false;
            playerInPromptRange = true;
            return;
        }

        playerInPromptRange = false;

        // Check if player is too far
        if (distance > maxDistance)
        {
            targetAlpha = 0f;
            isVisible = false;
            return;
        }

        // Check line of sight
        if (HasLineOfSight())
        {
            targetAlpha = 1f;
            isVisible = true;
        }
        else
        {
            targetAlpha = 0f;
            isVisible = false;
        }
    }

    private bool HasLineOfSight()
    {
        if (playerTransform == null) return false;

        // Direction from flower to player
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distance = directionToPlayer.magnitude;

        // Raycast from flower to player
        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, distance, blockingLayers))
        {
            // Hit something - check if it's the player or a wall
            if (hit.collider.CompareTag("Player"))
            {
                return true; // Player is visible
            }
            else
            {
                return false; // Wall blocks view
            }
        }

        // Nothing hit (no walls in the way)
        return true;
    }

    // ==================== RING ANIMATION ====================
    private void UpdateRingAnimation()
    {
        if (ringObject == null) return;

        // Rotation
        ringObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bobbing
        if (enableBobbing)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobOffset = Mathf.Sin(bobTimer) * bobAmount;
            ringObject.transform.position = transform.position + Vector3.up * (heightOffset + bobOffset);
        }
    }

    private void UpdateFade()
    {
        if (lineRenderer == null) return;

        // Smoothly fade to target alpha
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        SetRingAlpha(currentAlpha);
    }

    private void SetRingAlpha(float alpha)
    {
        if (lineRenderer == null) return;

        Color color = ringColor;
        color.a = alpha;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    // ==================== PUBLIC METHODS ====================
    /// <summary>
    /// Check if ring is currently visible
    /// </summary>
    public bool IsRingVisible()
    {
        return isVisible;
    }

    /// <summary>
    /// Force hide the ring
    /// </summary>
    public void ForceHide()
    {
        targetAlpha = 0f;
        isVisible = false;
    }

    /// <summary>
    /// Set ring color at runtime
    /// </summary>
    public void SetRingColor(Color newColor)
    {
        ringColor = newColor;
        if (lineRenderer != null && lineRenderer.material != null)
        {
            lineRenderer.material.color = newColor;
        }
    }

    // ==================== EDITOR GIZMOS ====================
    void OnDrawGizmosSelected()
    {
        // Draw min distance (prompt range)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistance);

        // Draw max distance (ring visible range)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}
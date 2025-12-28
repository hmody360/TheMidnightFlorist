using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class FlowerAudioManager : MonoBehaviour
{
    // ==================== AUDIO CLIPS ====================
    [Header("=== AUDIO CLIPS ===")]
    [Tooltip("Sound played when player picks up this flower")]
    public AudioClip pickupSound;

    [Tooltip("Sound played when all flowers are collected (optional)")]
    public AudioClip allCollectedSound;

    [Header("=== AUDIO SETTINGS ===")]
    [Tooltip("Volume for pickup sound (0-1)")]
    [Range(0f, 1f)]
    public float pickupVolume = 1f;

    [Tooltip("Volume for all collected sound (0-1)")]
    [Range(0f, 1f)]
    public float allCollectedVolume = 1f;

    // ==================== PRIVATE VARIABLES ====================
    private AudioSource audioSource;

    // ==================== UNITY METHODS ====================
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Configure audio source
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
    }

    // ==================== PUBLIC METHODS ====================
    /// <summary>
    /// Plays the pickup sound
    /// </summary>
    public void PlayPickupSound()
    {
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound, pickupVolume);
            Debug.Log("FlowerAudioManager: Playing pickup sound");
        }
        else if (pickupSound == null)
        {
            Debug.LogWarning("FlowerAudioManager: Pickup sound not assigned!");
        }
    }

    /// <summary>
    /// Plays the "all flowers collected" sound
    /// Call this from FlowerSpawnManager when all flowers are collected
    /// </summary>
    public void PlayAllCollectedSound()
    {
        if (allCollectedSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(allCollectedSound, allCollectedVolume);
            Debug.Log("FlowerAudioManager: Playing all collected sound");
        }
    }

    /// <summary>
    /// Plays the pickup sound at a specific position (useful for playing after destroy)
    /// Creates a temporary AudioSource at the position
    /// </summary>
    public static void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        // Create temporary game object for sound
        GameObject tempAudio = new GameObject("TempAudio_FlowerPickup");
        tempAudio.transform.position = position;

        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.spatialBlend = 1f; // 3D sound
        tempSource.Play();

        // Destroy after clip finishes
        Destroy(tempAudio, clip.length + 0.1f);
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Play Pickup Sound")]
    public void TestPickupSound()
    {
        PlayPickupSound();
    }

    [ContextMenu("Test: Play All Collected Sound")]
    public void TestAllCollectedSound()
    {
        PlayAllCollectedSound();
    }
}
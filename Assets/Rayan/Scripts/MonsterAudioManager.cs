using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ================================================================================
// MONSTER AUDIO MANAGER
// ================================================================================
// Handles all monster sounds and music transitions.
// 
// SETUP INSTRUCTIONS:
// 1. Add this script to your Monster GameObject (same object as MonsterAI)
// 2. Assign audio clips in the Inspector
// 3. For footsteps: Add Animation Events to walk/run animations that call:
//    - OnFootstepWalk() for walk animation
//    - OnFootstepRun() for run animation
//    (These are called through MonsterAnimationHandler)
//
// AUDIO SOURCES:
// This script creates its own AudioSources automatically:
// - One for sound effects (footsteps, screams)
// - One for breathing (looped)
// - One for chase music
// - Reference to scene music AudioSource (assign in inspector)
// ================================================================================

public class MonsterAudioManager : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static MonsterAudioManager Instance { get; private set; }

    // ==================== FOOTSTEP SOUNDS ====================
    [Header("=== FOOTSTEP SOUNDS ===")]
    [Tooltip("Walk footstep sounds (randomly selected)")]
    public AudioClip[] walkFootsteps;

    [Tooltip("Run footstep sounds (randomly selected)")]
    public AudioClip[] runFootsteps;

    [Tooltip("Walk footstep volume")]
    [Range(0f, 1f)]
    public float walkFootstepVolume = 0.5f;

    [Tooltip("Run footstep volume")]
    [Range(0f, 1f)]
    public float runFootstepVolume = 0.7f;

    // ==================== SCREAM SOUNDS ====================
    [Header("=== SCREAM SOUNDS ===")]
    [Tooltip("Scream when flower is taken (Alert state)")]
    public AudioClip alertScreamSound;

    [Tooltip("Scream when reaching flower and seeing it gone (Investigate state)")]
    public AudioClip investigateScreamSound;

    [Tooltip("Scream when monster first sees the player")]
    public AudioClip chaseStartScreamSound;

    [Tooltip("Screams during chase (plays randomly every few seconds). If empty, uses chaseStartScreamSound")]
    public AudioClip[] chaseLoopScreams;

    [Tooltip("Time between chase screams (seconds)")]
    public float chaseScreamInterval = 5f;

    [Tooltip("Random variation for chase scream interval (+/- seconds)")]
    public float chaseScreamIntervalVariation = 2f;

    [Tooltip("Alert scream volume")]
    [Range(0f, 1f)]
    public float alertScreamVolume = 1f;

    [Tooltip("Investigate scream volume")]
    [Range(0f, 1f)]
    public float investigateScreamVolume = 1f;

    [Tooltip("Chase scream volume")]
    [Range(0f, 1f)]
    public float chaseScreamVolume = 1f;

    // ==================== BREATHING SOUNDS ====================
    [Header("=== BREATHING SOUNDS (Optional) ===")]
    [Tooltip("Ambient breathing sound (looped)")]
    public AudioClip breathingSound;

    [Tooltip("Breathing volume")]
    [Range(0f, 1f)]
    public float breathingVolume = 0.3f;

    [Tooltip("Enable proximity-based breathing (louder when closer to player)")]
    public bool proximityBreathing = true;

    [Tooltip("Distance at which breathing is at full volume")]
    public float breathingFullVolumeDistance = 5f;

    [Tooltip("Distance at which breathing is silent")]
    public float breathingFadeDistance = 20f;

    // ==================== SEARCH SOUND ====================
    [Header("=== SEARCH SOUND (Optional) ===")]
    [Tooltip("Sound when monster is searching")]
    public AudioClip searchSound;

    [Tooltip("Search sound volume")]
    [Range(0f, 1f)]
    public float searchVolume = 0.6f;

    // ==================== CHASE MUSIC ====================
    [Header("=== CHASE MUSIC ===")]
    [Tooltip("Music that plays during chase")]
    public AudioClip chaseMusic;

    [Tooltip("Chase music volume")]
    [Range(0f, 1f)]
    public float chaseMusicVolume = 0.8f;

    [Tooltip("Fade duration for music transitions")]
    public float musicFadeDuration = 1f;

    // ==================== SCENE MUSIC ====================
    [Header("=== SCENE MUSIC ===")]
    [Tooltip("Reference to the scene's main music AudioSource")]
    public AudioSource sceneMusicSource;

    [Tooltip("Volume to lower scene music to during chase (0 = mute, 0.3 = quiet)")]
    [Range(0f, 1f)]
    public float sceneMusicChaseVolume = 0f;

    // ==================== DEBUG ====================
    [Header("=== DEBUG ===")]
    public bool showDebugLogs = true;

    // ==================== PRIVATE VARIABLES ====================
    private AudioSource sfxSource;          // For one-shot sounds (screams, footsteps)
    private AudioSource breathingSource;    // For looped breathing
    private AudioSource chaseMusicSource;   // For chase music

    private Transform player;
    private float originalSceneMusicVolume;
    private bool isChasing = false;
    private Coroutine sceneMusicFadeCoroutine;
    private Coroutine chaseMusicFadeCoroutine;
    private Coroutine chaseScreamCoroutine;

    private int lastWalkFootstepIndex = -1;
    private int lastRunFootstepIndex = -1;
    private int lastChaseScreamIndex = -1;

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
            Debug.LogWarning("MonsterAudioManager: Multiple instances detected!");
            Destroy(this);
            return;
        }
    }

    void Start()
    {
        // Create AudioSources
        SetupAudioSources();

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Store original scene music volume
        if (sceneMusicSource != null)
        {
            originalSceneMusicVolume = sceneMusicSource.volume;
        }

        // Start breathing if assigned
        if (breathingSound != null)
        {
            StartBreathing();
        }

        if (showDebugLogs)
        {
            Debug.Log("MonsterAudioManager: Initialized");
        }
    }

    void Update()
    {
        // Update proximity-based breathing volume
        if (proximityBreathing && breathingSource != null && breathingSource.isPlaying)
        {
            UpdateBreathingVolume();
        }
    }

    // ==================== SETUP ====================
    private void SetupAudioSources()
    {
        // SFX Source (for one-shots)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 1f; // 3D sound
        sfxSource.rolloffMode = AudioRolloffMode.Linear;
        sfxSource.minDistance = 1f;
        sfxSource.maxDistance = 30f;

        // Breathing Source (looped)
        breathingSource = gameObject.AddComponent<AudioSource>();
        breathingSource.playOnAwake = false;
        breathingSource.loop = true;
        breathingSource.spatialBlend = 1f; // 3D sound
        breathingSource.rolloffMode = AudioRolloffMode.Linear;
        breathingSource.minDistance = 1f;
        breathingSource.maxDistance = breathingFadeDistance;

        // Chase Music Source (2D for consistent volume)
        chaseMusicSource = gameObject.AddComponent<AudioSource>();
        chaseMusicSource.playOnAwake = false;
        chaseMusicSource.loop = true;
        chaseMusicSource.spatialBlend = 0f; // 2D sound
        chaseMusicSource.volume = 0f;
    }

    // ==================== FOOTSTEP METHODS ====================

    /// <summary>
    /// Play a random walk footstep sound
    /// </summary>
    public void PlayWalkFootstep()
    {
        if (walkFootsteps == null || walkFootsteps.Length == 0) return;

        AudioClip clip = GetRandomClip(walkFootsteps, ref lastWalkFootstepIndex);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, walkFootstepVolume);
        }
    }

    /// <summary>
    /// Play a random run footstep sound
    /// </summary>
    public void PlayRunFootstep()
    {
        if (runFootsteps == null || runFootsteps.Length == 0) return;

        AudioClip clip = GetRandomClip(runFootsteps, ref lastRunFootstepIndex);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, runFootstepVolume);
        }
    }

    private AudioClip GetRandomClip(AudioClip[] clips, ref int lastIndex)
    {
        if (clips.Length == 0) return null;
        if (clips.Length == 1) return clips[0];

        // Avoid repeating the same clip
        int newIndex;
        do
        {
            newIndex = Random.Range(0, clips.Length);
        } while (newIndex == lastIndex && clips.Length > 1);

        lastIndex = newIndex;
        return clips[newIndex];
    }

    // ==================== SCREAM METHODS ====================

    /// <summary>
    /// Play alert scream (when flower is taken)
    /// </summary>
    public void PlayAlertScream()
    {
        if (alertScreamSound == null)
        {
            if (showDebugLogs) Debug.Log("MonsterAudioManager: Alert scream sound not assigned");
            return;
        }

        sfxSource.PlayOneShot(alertScreamSound, alertScreamVolume);

        if (showDebugLogs)
        {
            Debug.Log("MonsterAudioManager: Playing Alert Scream");
        }
    }

    /// <summary>
    /// Play investigate scream (when reaching flower and seeing it gone)
    /// </summary>
    public void PlayInvestigateScream()
    {
        if (investigateScreamSound == null)
        {
            if (showDebugLogs) Debug.Log("MonsterAudioManager: Investigate scream sound not assigned");
            return;
        }

        sfxSource.PlayOneShot(investigateScreamSound, investigateScreamVolume);

        if (showDebugLogs)
        {
            Debug.Log("MonsterAudioManager: Playing Investigate Scream");
        }
    }

    // ==================== CHASE SCREAM METHODS ====================

    /// <summary>
    /// Play scream when monster first sees the player (start of chase)
    /// </summary>
    public void PlayChaseStartScream()
    {
        if (chaseStartScreamSound == null)
        {
            if (showDebugLogs) Debug.Log("MonsterAudioManager: Chase start scream sound not assigned");
            return;
        }

        sfxSource.PlayOneShot(chaseStartScreamSound, chaseScreamVolume);

        if (showDebugLogs)
        {
            Debug.Log("MonsterAudioManager: Playing Chase Start Scream");
        }
    }

    /// <summary>
    /// Start the chase scream loop (plays screams every few seconds during chase)
    /// </summary>
    public void StartChaseScreamLoop()
    {
        // Stop any existing loop
        StopChaseScreamLoop();

        // Start new loop
        chaseScreamCoroutine = StartCoroutine(ChaseScreamLoopCoroutine());

        if (showDebugLogs)
        {
            Debug.Log("MonsterAudioManager: Chase scream loop started");
        }
    }

    /// <summary>
    /// Stop the chase scream loop
    /// </summary>
    public void StopChaseScreamLoop()
    {
        if (chaseScreamCoroutine != null)
        {
            StopCoroutine(chaseScreamCoroutine);
            chaseScreamCoroutine = null;
        }
    }

    private IEnumerator ChaseScreamLoopCoroutine()
    {
        // Wait initial interval before first loop scream
        float waitTime = chaseScreamInterval + Random.Range(-chaseScreamIntervalVariation, chaseScreamIntervalVariation);
        yield return new WaitForSeconds(waitTime);

        while (isChasing)
        {
            // Play a chase scream
            PlayChaseLoopScream();

            // Wait for next scream
            waitTime = chaseScreamInterval + Random.Range(-chaseScreamIntervalVariation, chaseScreamIntervalVariation);
            waitTime = Mathf.Max(1f, waitTime); // Minimum 1 second
            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// Play a random chase loop scream
    /// </summary>
    private void PlayChaseLoopScream()
    {
        AudioClip clipToPlay = null;

        // If we have chase loop screams, pick a random one
        if (chaseLoopScreams != null && chaseLoopScreams.Length > 0)
        {
            clipToPlay = GetRandomClip(chaseLoopScreams, ref lastChaseScreamIndex);
        }
        // Otherwise, use the chase start scream (if assigned)
        else if (chaseStartScreamSound != null)
        {
            clipToPlay = chaseStartScreamSound;
        }

        // Play the clip if we have one
        if (clipToPlay != null)
        {
            sfxSource.PlayOneShot(clipToPlay, chaseScreamVolume);

            if (showDebugLogs)
            {
                Debug.Log($"MonsterAudioManager: Playing Chase Loop Scream");
            }
        }
    }

    // ==================== SEARCH SOUND ====================

    /// <summary>
    /// Play search sound
    /// </summary>
    public void PlaySearchSound()
    {
        if (searchSound == null)
        {
            if (showDebugLogs) Debug.Log("MonsterAudioManager: Search sound not assigned");
            return;
        }

        sfxSource.PlayOneShot(searchSound, searchVolume);

        if (showDebugLogs)
        {
            Debug.Log("MonsterAudioManager: Playing Search Sound");
        }
    }

    // ==================== BREATHING METHODS ====================

    /// <summary>
    /// Start breathing sound
    /// </summary>
    public void StartBreathing()
    {
        if (breathingSound == null || breathingSource == null) return;

        breathingSource.clip = breathingSound;
        breathingSource.volume = breathingVolume;
        breathingSource.Play();

        if (showDebugLogs)
        {
            Debug.Log("MonsterAudioManager: Breathing started");
        }
    }

    /// <summary>
    /// Stop breathing sound
    /// </summary>
    public void StopBreathing()
    {
        if (breathingSource != null)
        {
            breathingSource.Stop();
        }
    }

    private void UpdateBreathingVolume()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Calculate volume based on distance
        float t = Mathf.InverseLerp(breathingFadeDistance, breathingFullVolumeDistance, distance);
        float targetVolume = Mathf.Lerp(0f, breathingVolume, t);

        breathingSource.volume = targetVolume;
    }

    // ==================== CHASE MUSIC METHODS ====================

    /// <summary>
    /// Start chase music and lower scene music
    /// </summary>
    public void StartChaseMusic()
    {
        if (isChasing) return;
        isChasing = true;

        if (showDebugLogs)
        {
            Debug.Log("MonsterAudioManager: Starting chase music");
        }

        // Play chase start scream (when monster first sees player)
        PlayChaseStartScream();

        // Start chase scream loop
        StartChaseScreamLoop();

        // Fade out scene music
        if (sceneMusicSource != null)
        {
            if (sceneMusicFadeCoroutine != null) StopCoroutine(sceneMusicFadeCoroutine);
            sceneMusicFadeCoroutine = StartCoroutine(FadeAudioSource(sceneMusicSource, sceneMusicChaseVolume, musicFadeDuration));
        }

        // Fade in chase music
        if (chaseMusic != null && chaseMusicSource != null)
        {
            chaseMusicSource.clip = chaseMusic;
            chaseMusicSource.Play();

            if (chaseMusicFadeCoroutine != null) StopCoroutine(chaseMusicFadeCoroutine);
            chaseMusicFadeCoroutine = StartCoroutine(FadeAudioSource(chaseMusicSource, chaseMusicVolume, musicFadeDuration));
        }
    }

    /// <summary>
    /// Stop chase music and restore scene music
    /// </summary>
    public void StopChaseMusic()
    {
        if (!isChasing) return;
        isChasing = false;

        if (showDebugLogs)
        {
            Debug.Log("MonsterAudioManager: Stopping chase music");
        }

        // Stop chase scream loop
        StopChaseScreamLoop();

        // Fade in scene music
        if (sceneMusicSource != null)
        {
            if (sceneMusicFadeCoroutine != null) StopCoroutine(sceneMusicFadeCoroutine);
            sceneMusicFadeCoroutine = StartCoroutine(FadeAudioSource(sceneMusicSource, originalSceneMusicVolume, musicFadeDuration));
        }

        // Fade out chase music
        if (chaseMusicSource != null)
        {
            if (chaseMusicFadeCoroutine != null) StopCoroutine(chaseMusicFadeCoroutine);
            chaseMusicFadeCoroutine = StartCoroutine(FadeAudioSourceAndStop(chaseMusicSource, 0f, musicFadeDuration));
        }
    }

    /// <summary>
    /// Check if chase music is playing
    /// </summary>
    public bool IsChaseMusicPlaying()
    {
        return isChasing;
    }

    // ==================== AUDIO FADE COROUTINES ====================

    private IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private IEnumerator FadeAudioSourceAndStop(AudioSource source, float targetVolume, float duration)
    {
        yield return FadeAudioSource(source, targetVolume, duration);

        if (source != null)
        {
            source.Stop();
        }
    }

    // ==================== PUBLIC HELPER METHODS ====================

    /// <summary>
    /// Set scene music source at runtime
    /// </summary>
    public void SetSceneMusicSource(AudioSource musicSource)
    {
        sceneMusicSource = musicSource;
        if (sceneMusicSource != null)
        {
            originalSceneMusicVolume = sceneMusicSource.volume;
        }
    }

    /// <summary>
    /// Play a custom one-shot sound
    /// </summary>
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // ==================== EDITOR TESTING ====================
    [ContextMenu("Test: Play Walk Footstep")]
    public void TestWalkFootstep()
    {
        PlayWalkFootstep();
    }

    [ContextMenu("Test: Play Run Footstep")]
    public void TestRunFootstep()
    {
        PlayRunFootstep();
    }

    [ContextMenu("Test: Play Alert Scream")]
    public void TestAlertScream()
    {
        PlayAlertScream();
    }

    [ContextMenu("Test: Play Investigate Scream")]
    public void TestInvestigateScream()
    {
        PlayInvestigateScream();
    }

    [ContextMenu("Test: Play Chase Start Scream")]
    public void TestChaseStartScream()
    {
        PlayChaseStartScream();
    }

    [ContextMenu("Test: Start Chase Music")]
    public void TestStartChaseMusic()
    {
        StartChaseMusic();
    }

    [ContextMenu("Test: Stop Chase Music")]
    public void TestStopChaseMusic()
    {
        StopChaseMusic();
    }

    [ContextMenu("Test: Play Search Sound")]
    public void TestSearchSound()
    {
        PlaySearchSound();
    }
}
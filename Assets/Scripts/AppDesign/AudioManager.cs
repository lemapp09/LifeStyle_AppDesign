using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    // This static method is called when the Unity domain reloads, ensuring
    // the static _instance variable is reset.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        _instance = null;
    }

    // Singleton pattern for easy access from other scripts.
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Use the more performant FindAnyObjectByType method.
                _instance = FindAnyObjectByType<AudioManager>();
                if (_instance == null)
                {
                    Debug.LogError("AudioManager instance not found. Make sure there is one in your scene.");
                }
            }
            return _instance;
        }
    }

    [Header("Audio Mixer and Volume")]
    [Tooltip("Assign your Audio Mixer here.")]
    [SerializeField] private AudioMixer _audioMixer;
    
    // Exposed parameters from the Audio Mixer.
    // Ensure these strings match the exposed parameter names exactly.
    [Tooltip("The name of the exposed volume parameter for the Master group.")]
    [SerializeField] private string _masterVolumeParameter = "Volume_Master";
    [Tooltip("The name of the exposed volume parameter for the Background group.")]
    [SerializeField] private string _backgroundVolumeParameter = "Volume_Background";
    [Tooltip("The name of the exposed volume parameter for the SFX group.")]
    [SerializeField] private string _sfxVolumeParameter = "Volume_SFX";

    // Private fields to store previous volume values before ducking.
    private float _previousMasterVolume;
    private float _previousBackgroundVolume;

    [Header("Background Music")]
    [Tooltip("Assign the two Audio Sources for background music here.")]
    [SerializeField] private AudioSource _backgroundAudioSource1;
    [SerializeField] private AudioSource _backgroundAudioSource2;
    [Tooltip("The array of background music clips to play.")]
    [SerializeField] private AudioClip[] _backgroundClips;
    [Tooltip("The duration in seconds for the crossfade between background tracks.")]
    [SerializeField] private float _backgroundFadeDuration = 3f;

    private int _currentBackgroundClipIndex = 0;
    private AudioSource _activeBackgroundSource;
    private AudioSource _inactiveBackgroundSource;
    private Coroutine _fadeCoroutine;

    [Header("Sound Effects (SFX)")]
    [Tooltip("Assign the Audio Source for SFX here.")]
    [SerializeField] private AudioSource _sfxAudioSource;
    [Tooltip("The single SFX clip to be used for different tones.")]
    [SerializeField] private AudioClip _sfxClip;
    [Tooltip("The pitch values for the 9 different tones.")]
    // The pitch values correspond to the semitone steps of a chromatic scale,
    // where pitch = 2^(n/12) and n is the number of semitones from the base pitch.
    [SerializeField] private float[] _sfxPitches = { 0.79370f, 0.84090f, 0.89090f, 0.94387f, 1.0f, 1.05946f, 1.12246f, 1.18921f, 1.25992f };

    private void Awake()
    {
        // Enforce the singleton pattern.
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Initialize the active and inactive audio sources.
        _activeBackgroundSource = _backgroundAudioSource1;
        _inactiveBackgroundSource = _backgroundAudioSource2;
    }

    private void Start()
    {
        // Start the continuous background music playback.
        StartCoroutine(ContinuousBackgroundMusicLoop());
    }

    private IEnumerator ContinuousBackgroundMusicLoop()
    {
        if (_backgroundClips == null || _backgroundClips.Length == 0)
        {
            Debug.LogWarning("No background music clips assigned.");
            yield break;
        }

        // Play the first clip immediately.
        _activeBackgroundSource.clip = _backgroundClips[_currentBackgroundClipIndex];
        _activeBackgroundSource.Play();

        // Loop continuously to manage playback and crossfading.
        while (true)
        {
            // Check if the current clip is nearing its end.
            if (_activeBackgroundSource.time >= _activeBackgroundSource.clip.length - _backgroundFadeDuration)
            {
                // Increment the clip index, looping back to the start if necessary.
                _currentBackgroundClipIndex = (_currentBackgroundClipIndex + 1) % _backgroundClips.Length;
                
                // Set up the next clip on the inactive source.
                _inactiveBackgroundSource.clip = _backgroundClips[_currentBackgroundClipIndex];
                _inactiveBackgroundSource.Play();

                // Start the crossfade.
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(CrossfadeBackgroundMusic(_backgroundFadeDuration));
            }
            yield return null;
        }
    }

    private IEnumerator CrossfadeBackgroundMusic(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            float t = timer / duration;
            // Fade out the active source and fade in the inactive source.
            // Using Mathf.Lerp for smooth transition.
            _activeBackgroundSource.volume = Mathf.Lerp(1f, 0f, t);
            _inactiveBackgroundSource.volume = Mathf.Lerp(0f, 1f, t);
            
            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure the fade is complete and volumes are set to their final values.
        _activeBackgroundSource.volume = 0f;
        _inactiveBackgroundSource.volume = 1f;

        // Stop the old source and switch the active/inactive roles.
        _activeBackgroundSource.Stop();
        (_activeBackgroundSource, _inactiveBackgroundSource) = (_inactiveBackgroundSource, _activeBackgroundSource);
    }

    // Public methods to control the exposed volumes from other scripts or UI.
    // The volume parameter should be between 0.0 and 1.0.
    public void SetMasterVolume(float volume)
    {
        SetVolume(_masterVolumeParameter, volume);
    }

    public void SetBackgroundVolume(float volume)
    {
        SetVolume(_backgroundVolumeParameter, volume);
    }

    public void SetSFXVolume(float volume)
    {
        SetVolume(_sfxVolumeParameter, volume);
    }

    // Gets the current master volume as a linear value (0.0 to 1.0).
    public float GetMasterVolume()
    {
        return GetVolume(_masterVolumeParameter);
    }

    // Gets the current background volume as a linear value (0.0 to 1.0).
    public float GetBackgroundVolume()
    {
        return GetVolume(_backgroundVolumeParameter);
    }

    // Gets the current SFX volume as a linear value (0.0 to 1.0).
    public float GetSFXVolume()
    {
        return GetVolume(_sfxVolumeParameter);
    }

    // Reduces the master volume to 25% of its current value.
    public void DuckMasterVolume()
    {
        // Get the current volume in decibels.
        float currentDb;
        _audioMixer.GetFloat(_masterVolumeParameter, out currentDb);

        // Convert the current decibel value to a linear volume (0-1) and store it.
        _previousMasterVolume = Mathf.Pow(10, currentDb / 20f);

        // Calculate the new volume (25% of the previous value).
        float newVolume = _previousMasterVolume * 0.1f;

        // Set the master volume to the new, lower value.
        SetMasterVolume(newVolume);
    }

    // Restores the master volume to its previous value.
    public void RestoreMasterVolume()
    {
        // Set the master volume back to the stored previous value.
        SetMasterVolume(_previousMasterVolume);
    }
    
    // Reduces the background volume to 25% of its current value.
    public void DuckBackgroundVolume()
    {
        // Get the current volume in decibels.
        float currentDb;
        _audioMixer.GetFloat(_backgroundVolumeParameter, out currentDb);

        // Convert the current decibel value to a linear volume (0-1) and store it.
        _previousBackgroundVolume = Mathf.Pow(10, currentDb / 20f);

        // Calculate the new volume (25% of the previous value).
        float newVolume = _previousBackgroundVolume * 0.25f;

        // Set the background volume to the new, lower value.
        SetBackgroundVolume(newVolume);
    }

    // Restores the background volume to its previous value.
    public void RestoreBackgroundVolume()
    {
        // Set the background volume back to the stored previous value.
        SetBackgroundVolume(_previousBackgroundVolume);
    }

    // Helper method to set the volume on the Audio Mixer.
    private void SetVolume(string parameterName, float volume)
    {
        // Convert the linear volume (0-1) to a logarithmic decibel scale.
        // A volume of 0 results in -80dB, which is considered silence.
        _audioMixer.SetFloat(parameterName, Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
    }

    // Helper method to get the volume from the Audio Mixer and convert it to linear scale.
    private float GetVolume(string parameterName)
    {
        float currentDb;
        _audioMixer.GetFloat(parameterName, out currentDb);
        // Convert the decibel value back to a linear volume (0-1).
        return Mathf.Pow(10, currentDb / 20f);
    }

    // Plays the SFX clip with a specific pitch.
    // pitchIndex should be between 0 and 8.
    public void PlaySFX(int pitchIndex)
    {
        // Check if SFX is already playing and stop it before playing a new one.
        if (_sfxAudioSource.isPlaying)
        {
            _sfxAudioSource.Stop();
        }

        if (pitchIndex < 0 || pitchIndex >= _sfxPitches.Length)
        {
            Debug.LogError("Invalid pitch index. Please use an index between 0 and 8.");
            return;
        }

        if (_sfxClip == null)
        {
            Debug.LogWarning("SFX clip not assigned.");
            return;
        }

        _sfxAudioSource.clip = _sfxClip;
        _sfxAudioSource.pitch = _sfxPitches[pitchIndex];
        double currentTime = AudioSettings.dspTime;
        _sfxAudioSource.PlayScheduled(currentTime);
        _sfxAudioSource.SetScheduledEndTime(currentTime + 0.5f);
    }
}
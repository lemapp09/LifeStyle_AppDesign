using UnityEngine;
using UnityEngine.UIElements;

public class SettingsManager : MonoBehaviour
{
    
    private Slider _masterVolumeSlider, _backgroundVolumeSlider, _sfxVolumeSlider;
    
    // These methods provide a public interface for UI elements to interact with the AudioManager.
    // This separates the settings logic from the core audio functionality.

    /// <summary>
    /// Gets the current master volume from the AudioManager.
    /// </summary>
    /// <returns>The master volume as a linear value (0.0 to 1.0).</returns>
    public float GetMasterVolume()
    {
        return AudioManager.Instance.GetMasterVolume();
    }

    /// <summary>
    /// Sets the master volume in the AudioManager.
    /// </summary>
    /// <param name="volume">The new volume to set, from 0.0 to 1.0.</param>
    public void SetMasterVolume(float volume)
    {
        AudioManager.Instance.SetMasterVolume(volume);
    }

    /// <summary>
    /// Gets the current background volume from the AudioManager.
    /// </summary>
    /// <returns>The background volume as a linear value (0.0 to 1.0).</returns>
    public float GetBackgroundVolume()
    {
        return AudioManager.Instance.GetBackgroundVolume();
    }

    /// <summary>
    /// Sets the background volume in the AudioManager.
    /// </summary>
    /// <param name="volume">The new volume to set, from 0.0 to 1.0.</param>
    public void SetBackgroundVolume(float volume)
    {
        AudioManager.Instance.SetBackgroundVolume(volume);
    }

    /// <summary>
    /// Gets the current SFX volume from the AudioManager.
    /// </summary>
    /// <returns>The SFX volume as a linear value (0.0 to 1.0).</returns>
    public float GetSFXVolume()
    {
        return AudioManager.Instance.GetSFXVolume();
    }

    /// <summary>
    /// Sets the SFX volume in the AudioManager.
    /// </summary>
    /// <param name="volume">The new volume to set, from 0.0 to 1.0.</param>
    public void SetSFXVolume(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
    }

    public void SetSliderElements(Slider masterVolumeSlider, Slider backgroundVolumeSlider, Slider sfxVolumeSlider)
    {
        _masterVolumeSlider = masterVolumeSlider;
        _backgroundVolumeSlider = backgroundVolumeSlider;
        _sfxVolumeSlider = sfxVolumeSlider;
    }

    public void SetAllSliders()
    {
        _masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
        _backgroundVolumeSlider.value = AudioManager.Instance.GetBackgroundVolume();
        _sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
    }
    
    public void OnMasterVolumeChanged(ChangeEvent<float> evt)
    {
        // The new value of the slider is available via evt.newValue
        float newVolume = evt.newValue;
        SetMasterVolume(newVolume);
    }
    
    public void OnBackgroundVolumeChanged(ChangeEvent<float> evt)
    {
        // The new value of the slider is available via evt.newValue
        float newVolume = evt.newValue;
        SetBackgroundVolume(newVolume);
    }
    
    public void OnSFXVolumeChanged(ChangeEvent<float> evt)
    {
        // The new value of the slider is available via evt.newValue
        float newVolume = evt.newValue;
        SetSFXVolume(newVolume);
    }
}
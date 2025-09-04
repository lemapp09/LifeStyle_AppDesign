using UnityEngine;
using UnityEngine.UIElements;

public class SettingsManager : MonoBehaviour
{
    
    private Slider _masterVolumeSlider, _backgroundVolumeSlider, _sfxVolumeSlider;
    private Label _masterVolumeLabel, _backgroundVolumeLabel, _sfxVolumeLabel;
    
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
        _masterVolumeLabel.text = GetMasterVolume().ToString();
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
        _backgroundVolumeLabel.text = volume.ToString();
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
        _sfxVolumeLabel.text = volume.ToString();
    }

    public void SetSliderElements(Slider masterVolumeSlider, Slider backgroundVolumeSlider, Slider sfxVolumeSlider,
        Label masterVolumeLabel, Label backgroundVolumeLabel, Label sfxVolumeLabel)
    {
        _masterVolumeSlider = masterVolumeSlider;
        _masterVolumeLabel = masterVolumeLabel;
        _backgroundVolumeSlider = backgroundVolumeSlider;
        _backgroundVolumeLabel = backgroundVolumeLabel;
        _sfxVolumeSlider = sfxVolumeSlider;
        _sfxVolumeLabel = sfxVolumeLabel;
    }

    public void SetAllSliders()
    {
        float masterVolume = GetMasterVolume();
        _masterVolumeSlider.value = masterVolume;
        _masterVolumeLabel.text = "";
        float backgroundVolume = GetBackgroundVolume();
        _backgroundVolumeSlider.value = backgroundVolume;
        _backgroundVolumeLabel.text = "";
        float sfxVolume = GetSFXVolume();
        _sfxVolumeSlider.value = sfxVolume;
        _sfxVolumeLabel.text = "";
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
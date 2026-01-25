using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public Scrollbar volumeSlider;
    public Scrollbar sfxSlider;

    void Start()
    {
        // Load saved values
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFX", 1f);
    }

    public void SetVolume(float value)
    {
        PlayerPrefs.SetFloat("Volume", value);
        AudioListener.volume = value;
    }

    public void SetSFX(float value)
    {
        PlayerPrefs.SetFloat("SFX", value);
        
    }

    public void ResetSettings()
    {
        volumeSlider.value = 1f;
        sfxSlider.value = 1f;

        SetVolume(1f);
        SetSFX(1f);
    }
}

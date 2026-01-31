using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public Scrollbar volumeSlider;
    public Scrollbar sfxSlider;
    public AudioSource sfxSource; 
    void Start()
    {
        
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFX", 1f);

        volumeSlider.value = volume;
        sfxSlider.value = sfx;

        
        AudioListener.volume = volume;
        sfxSource.volume = sfx;
    }

    public void SetVolume(float value)
    {
        PlayerPrefs.SetFloat("Volume", value);
        AudioListener.volume = value;
    }

    public void SetSFX(float value)
    {
        PlayerPrefs.SetFloat("SFX", value);
        sfxSource.volume = value;
    }

    public void ResetSettings()
    {
        volumeSlider.value = 1f;
        sfxSlider.value = 1f;

        SetVolume(1f);
        SetSFX(1f);
    }
}

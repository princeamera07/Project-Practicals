using UnityEngine;

public class ButtonSFX : MonoBehaviour
{
    public AudioSource sfxSource;

    public void PlayClick()
    {
        sfxSource.PlayOneShot(sfxSource.clip, sfxSource.volume);
    }
}

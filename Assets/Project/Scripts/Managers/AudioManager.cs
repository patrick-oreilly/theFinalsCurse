using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    [Header("Global SFX")]
    public AudioSource sfxSource;
    public AudioClip coinSound;
    public AudioClip winSound;
    public AudioClip buttonSound;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: Keep music between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMusic(backgroundMusic);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip != null && musicSource != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayCoinSound() => PlaySFX(coinSound, 0.8f);
    public void PlayWinSound() => PlaySFX(winSound, 1f);
    public void PlayButtonSound() => PlaySFX(buttonSound, 1f);

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null) musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = volume;
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }
}

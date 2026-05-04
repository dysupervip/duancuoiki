using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("SFX")]
    [SerializeField] private AudioClip gunShot;
    [SerializeField] private AudioClip oilPickup;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip victoryClip;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayGunShot()
    {
        sfxSource.PlayOneShot(gunShot);
    }

    public void PlayOilPickup()
    {
        sfxSource.PlayOneShot(oilPickup);
    }

    public void StartGameplayMusic()
    {
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayGameOver()
    {
        musicSource.Stop();
        sfxSource.PlayOneShot(gameOverClip);
    }

    public void PlayVictory()
    {
        musicSource.Stop();
        sfxSource.PlayOneShot(victoryClip);
    }
}
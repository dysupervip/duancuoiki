using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("SFX")]
    [SerializeField] private AudioClip gunShot;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip beamClip;
    [SerializeField] private AudioClip dualClip;
    [SerializeField] private AudioClip petClip;

    [SerializeField] private AudioClip oilPickup;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip victoryClip;

    [Header("Boss")]
    [SerializeField] private AudioClip bossBeamClip;
    [SerializeField] private AudioClip bossOrbClip;
    [SerializeField] private AudioClip bossFireClip;
    [SerializeField] private AudioClip miniBossMeleeClip;
    [SerializeField] private AudioClip miniBossSlamClip;
    [SerializeField] private AudioClip miniBossThrowClip;


    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    

    void Start()
    {
        StartGameplayMusic();
    }
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
    public void PlayReload()
    {
        if (sfxSource != null && reloadClip != null)
        {
            sfxSource.PlayOneShot(reloadClip);
        }
    }

   public void PlayOilPickup()
    {
        if (sfxSource != null && oilPickup != null)
            sfxSource.PlayOneShot(oilPickup);
    }
    public void PlayDash()
    {
        if (sfxSource != null && dashClip != null)
            sfxSource.PlayOneShot(dashClip);
    }
    public void PlayPet()
    {
        if (sfxSource != null && petClip != null)
            sfxSource.PlayOneShot(petClip);
    }
    public void PlayBeam()
    {
        if (sfxSource != null && beamClip != null)
            sfxSource.PlayOneShot(beamClip);
    }
    public void PlayDual()
    {
        if (sfxSource != null && dualClip != null)
            sfxSource.PlayOneShot(dualClip);
    }
    public void PlayBossBeam()
    {
        if (sfxSource != null && bossBeamClip != null)
            sfxSource.PlayOneShot(bossBeamClip);
    }
    public void PlayBossOrb()
    {
        if (sfxSource != null && bossOrbClip != null)
            sfxSource.PlayOneShot(bossOrbClip);
    }
    public void PlayBossFire()
    {
        if (sfxSource != null && bossFireClip != null)
            sfxSource.PlayOneShot(bossFireClip);
    }
    public void PlayMiniBossMelee()
    {
        if (sfxSource != null && miniBossMeleeClip != null)
            sfxSource.PlayOneShot(miniBossMeleeClip);
    }
    public void PlayMiniBossSlam()
    {
        if (sfxSource != null && miniBossSlamClip != null)
            sfxSource.PlayOneShot(miniBossSlamClip);
    }
    public void PlayMiniBossThrow()
    {
        if (sfxSource != null && miniBossThrowClip != null)
            sfxSource.PlayOneShot(miniBossThrowClip);
    }
    public void StartGameplayMusic()
    {
        if (musicSource == null) return;
        if (backgroundMusic == null) return;
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
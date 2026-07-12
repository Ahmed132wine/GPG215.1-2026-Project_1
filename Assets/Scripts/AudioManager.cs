using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton Instance
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("The AudioSource component used to play weapon sounds.")]
    public AudioSource weaponAudioSource;

    [Header("Weapon Sound Clips")]
    public AudioClip rifleShotClip;
    public AudioClip sniperShotClip;

    [Header("Feedback Sound Clips")]
    public AudioClip playerDamageClip;
    public AudioClip playerDeathClip;
    public AudioClip enemyDeathClip;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;


        if (weaponAudioSource == null)
        {
            weaponAudioSource = gameObject.AddComponent<AudioSource>();
            weaponAudioSource.playOnAwake = false;
        }
    }

    public void PlayRifleShot()
    {
        if (weaponAudioSource != null && rifleShotClip != null)
        {

            weaponAudioSource.PlayOneShot(rifleShotClip);
        }
    }

    public void PlaySniperShot()
    {
        if (weaponAudioSource != null && sniperShotClip != null)
        {
            weaponAudioSource.PlayOneShot(sniperShotClip);
        }
    }


    public void PlayPlayerDamage()
    {
        if (weaponAudioSource != null && playerDamageClip != null)
        {
            weaponAudioSource.PlayOneShot(playerDamageClip);
        }
    }

    public void PlayPlayerDeath()
    {
        if (weaponAudioSource != null && playerDeathClip != null)
        {
            weaponAudioSource.PlayOneShot(playerDeathClip);
        }
    }

    public void PlayEnemyDeath()
    {
        if (weaponAudioSource != null && enemyDeathClip != null)
        {
            weaponAudioSource.PlayOneShot(enemyDeathClip);
        }
    }
}

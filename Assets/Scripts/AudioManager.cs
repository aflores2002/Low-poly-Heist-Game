using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Effects")]
    public AudioClip cashRegisterClip;
    public AudioClip carDriveOffClip;

    [Header("Audio Sources")]
    public AudioSource generalSFXSource;     // Manually assigned in the Inspector
    private AudioSource carAudioSource;      // Created at runtime for fade control

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Check that the generalSFXSource is assigned
        if (generalSFXSource == null)
        {
            Debug.LogWarning("AudioManager: General SFX AudioSource is not assigned!");
        }

        // Create a dedicated runtime audio source for car fade-out
        carAudioSource = gameObject.AddComponent<AudioSource>();
        carAudioSource.playOnAwake = false;
        carAudioSource.loop = false;
    }

    /// <summary>
    /// Plays a one-shot sound using the general audio source.
    /// </summary>
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip != null && generalSFXSource != null)
        {
            generalSFXSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayCashRegister()
    {
        PlayOneShot(cashRegisterClip, 0.4f);
    }

    public void PlayCarDriveOff()
    {
        if (carDriveOffClip != null)
        {
            StartCoroutine(PlayCarDriveOffPartial());
        }
    }

    private IEnumerator PlayCarDriveOffPartial()
    {
        carAudioSource.clip = carDriveOffClip;
        carAudioSource.volume = 1f;
        carAudioSource.Play();

        yield return new WaitForSeconds(3f);

        float fadeTime = 1f;
        float startVolume = 1f;
        float endVolume = 0.2f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, endVolume, elapsed / fadeTime);
            carAudioSource.volume = newVolume;
            yield return null;
        }

        carAudioSource.Stop();
    }
}

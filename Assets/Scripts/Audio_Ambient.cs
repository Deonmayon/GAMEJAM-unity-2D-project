using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioClip ambientMusic;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = ambientMusic;
        audioSource.loop = true;
        audioSource.volume = 0.5f;
        audioSource.Play();
    }
}
using System.Collections.Generic;
using UnityEngine;

public class JumpscareMgmt : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> sounds;
    [SerializeField, ReadOnly] private bool isPlaying;
    [SerializeField, ReadOnly] private bool soundPlayed;
    
    [SerializeField] private float jumpscareCooldown = 30f;
    [SerializeField, ReadOnly] private float elapsedTime;

    void Start()
    {
        Restart();
        ResetJumpscare();
    }

    void Update()
    {
        if (elapsedTime < jumpscareCooldown)
            elapsedTime += Time.deltaTime;

        if (soundPlayed) return;

        if (audioSource.isPlaying) {
            isPlaying = true;

        } else if (isPlaying) {
            SortSound();
            isPlaying = false;
            soundPlayed = true;
        }
    }

    void SortSound()
    {
        int chosen = Random.Range(0, sounds.Count);
        audioSource.clip = sounds[chosen];
    }

    // dar restart sempre que mirage teleportar
    public void Restart()
    {
        SortSound();
        isPlaying = false;
        soundPlayed = false;
    }

    // dar reset sempre que slender teleportar
    public void ResetJumpscare()
    {
        elapsedTime = jumpscareCooldown;
    }

    public void MakeJumpscare()
    {
        if (!audioSource.isPlaying && elapsedTime >= jumpscareCooldown) {
            SortSound();
            audioSource.Play();
        }
    }
}

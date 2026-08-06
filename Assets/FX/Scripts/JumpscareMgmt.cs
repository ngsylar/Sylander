using System.Collections.Generic;
using UnityEngine;

public class JumpscareMgmt : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> sounds;
    [SerializeField, ReadOnly] private bool isPlaying;
    [SerializeField, ReadOnly] private bool soundPlayed;
    
    [SerializeField] private float realJumpscareCooldown = 90f;
    [SerializeField, ReadOnly] private float elapsedTime;

    void Start()
    {
        Restart();
        ResetRealJumpscare();
    }

    void Update()
    {
        if (elapsedTime < realJumpscareCooldown)
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

    public void MakeJumpscare(SlenderMirage mirage)
    {
        if (!isPlaying && !soundPlayed) {
            mirage.IncrementJumpscare();
            audioSource.Play();
        }
    }

    // dar reset sempre que slender teleportar
    public void ResetRealJumpscare()
    {
        elapsedTime = realJumpscareCooldown;
    }

    public void MakeRealJumpscare()
    {
        if (!audioSource.isPlaying && elapsedTime >= realJumpscareCooldown) {
            SortSound();
            audioSource.Play();
        }
    }
}

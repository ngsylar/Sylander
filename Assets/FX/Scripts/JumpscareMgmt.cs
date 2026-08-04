using System.Collections.Generic;
using UnityEngine;

public class JumpscareMgmt : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> sounds;
    [SerializeField, ReadOnly] private bool isPlaying;
    [SerializeField, ReadOnly] private bool soundPlayed;

    void Start()
    {
        SortSound();
        isPlaying = false;
        soundPlayed = false;
    }

    void Update()
    {
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

    // sylar: dar restart smp que teleportar
    public void Restart()
    {
        Start();
    }

    public void MakeJumpscare(SlenderMirage mirage)
    {
        if (!isPlaying && !soundPlayed) {
            mirage.IncrementJumpscare();
            audioSource.Play();
        }
    }
}

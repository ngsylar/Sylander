using System.Collections.Generic;
using UnityEngine;

public class BackgroundMgmt : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> sounds;
    private bool isDead;

    void Start()
    {
        isDead = false;
        audioSource.loop = true;
    }

    public void UpdateMusic(int pageCount)
    {
        switch (pageCount) {
            case 1:
                audioSource.clip = sounds[0];
                audioSource.Play();
                break;
            case 3:
                audioSource.clip = sounds[1];
                audioSource.Play();
                break;
            case 5:
                audioSource.clip = sounds[2];
                audioSource.Play();
                break;
            case 7:
                audioSource.clip = sounds[3];
                audioSource.Play();
                break;
            case 8:
                audioSource.Stop();
                break;
            default: break;
        }
    }

    public void PlayScreamSound()
    {
        if (isDead) return;
        audioSource.clip = sounds[4];
        audioSource.loop = false;
        audioSource.Play();
        isDead = true;
    }
}

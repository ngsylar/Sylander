using System.Collections;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioSource audioWalking;
    public AudioSource audioRunning;
    public float fadeSeconds;

    private class AudioMgmt
    {
        public AudioSource audio;
        public float volumeMax;
        public bool isFadingIn;
        public bool isFadingOut;
        public float fadeDuration;
        public float currentTime;
        public float currentVolume;


        public AudioMgmt(AudioSource audio, float volume, float duration)
        {
            this.audio = audio;
            volumeMax = volume;
            isFadingIn = false;
            isFadingOut = false;
            fadeDuration = duration;
            currentTime = duration;
            currentVolume = volume;
        }
    }

    private AudioMgmt walking;
    private AudioMgmt running;
    [SerializeField, ReadOnly] private int currentState = 0;

    void Awake()
    {
        walking = new(audioWalking, audioWalking.volume, fadeSeconds);
        running = new(audioRunning, audioRunning.volume, fadeSeconds);
    }

    public void HandleSound(float inputZ, float inputX, bool inputRun)
    {
        bool isMoving = Mathf.Abs(inputZ) > 0.1f || Mathf.Abs(inputX) > 0.1f;

        int newState = 0;                           // idle
        if (isMoving && inputRun) newState = 2;     // running
        else if (isMoving) newState = 1;            // walking

        if (newState == currentState) return;
        currentState = newState;

        switch (currentState)
        {
            case 0:
                StopAudio(walking);
                StopAudio(running);
                break;
            case 1:
                StopAndPlay(running, walking);
                break;
            case 2:
                StopAndPlay(walking, running);
                break;
        }
    }

    void StopAndPlay(AudioMgmt movement1, AudioMgmt movement2)
    {
        StopAudio(movement1);
        PlayAudio(movement2);
    }

    void PlayAudio(AudioMgmt movement)
    {
        if (movement.isFadingOut)
            movement.isFadingOut = false;
        else if (movement.audio.isPlaying) return;

        movement.isFadingIn = true;
        movement.currentTime = movement.fadeDuration - movement.currentTime;
        movement.audio.Play();
    }

    void StopAudio(AudioMgmt movement)
    {
        if (movement.isFadingIn)
            movement.isFadingIn = false;
        else if (!movement.audio.isPlaying) return;

        movement.currentTime = movement.fadeDuration - movement.currentTime;
        movement.isFadingOut = true;
    }

    void FadeIn(AudioMgmt movement)
    {
        if (movement.currentTime < movement.fadeDuration) {
            movement.currentTime += Time.deltaTime;
            float t = movement.currentTime / movement.fadeDuration;
            movement.currentVolume = Mathf.Lerp(movement.currentVolume, movement.volumeMax, t);
            movement.audio.volume = movement.currentVolume;
        }
        else {
            movement.audio.volume = movement.currentVolume = movement.volumeMax;
            movement.currentTime = movement.fadeDuration;
            movement.isFadingIn = false;
        }
    }

    void FadeOut(AudioMgmt movement)
    {
        if (movement.currentTime < movement.fadeDuration) {
            movement.currentTime += Time.deltaTime;
            float t = movement.currentTime / movement.fadeDuration;
            movement.currentVolume = Mathf.Lerp(movement.currentVolume, 0f, t);
            movement.audio.volume = movement.currentVolume;
        }
        else {
            movement.audio.volume = movement.currentVolume = 0f;
            movement.currentTime = movement.fadeDuration;
            movement.isFadingOut = false;
            movement.audio.Stop();
        }
    }

    void Update()
    {
        if (walking.isFadingIn) FadeIn(walking);
        if (walking.isFadingOut) FadeOut(walking);
        if (running.isFadingIn) FadeIn(running);
        if (running.isFadingOut) FadeOut(running);
    }
}

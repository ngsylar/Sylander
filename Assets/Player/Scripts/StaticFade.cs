using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioFadeIn : MonoBehaviour
{
    public SlenderManAI slenderman;

    public float initialVolume;

    public AudioSource radioNoise;

    public AudioSource slenderStatic;
    private bool slenderWasPlaying = false;

    void Update()
    {
        if (slenderStatic.isPlaying && (slenderStatic.time >= 0.5f))
        {
            if (radioNoise.isPlaying) radioNoise.Stop();
            slenderWasPlaying = true;
            return;
        }
        if (slenderWasPlaying)
        {
            radioNoise.Play();
            slenderWasPlaying = false;
        }
        float t = Mathf.InverseLerp(slenderman.InterferenceMin, slenderman.InterferenceMax, slenderman.DistanceToPlayer);
        radioNoise.volume = Mathf.Lerp(initialVolume, 1f, t);
    }
}
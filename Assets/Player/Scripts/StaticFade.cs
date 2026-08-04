using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class StaticFade : MonoBehaviour
{
    public SlenderManAI slenderman;
    public SlenderMirage mirage;

    public float initialVolume;

    public AudioSource radioNoise;

    public AudioSource slenderStatic;
    private bool slenderWasPlaying = false;

    private float DistanceToPlayer
    {
        get => mirage.gameObject.activeSelf
            ? Mathf.Min(mirage.DistanceToPlayer, slenderman.DistanceToPlayer)
            : slenderman.DistanceToPlayer;
    }

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
        float t = Mathf.InverseLerp(slenderman.InterferenceMin, slenderman.InterferenceMax, DistanceToPlayer);
        radioNoise.volume = Mathf.Lerp(initialVolume, 1f, t);
    }
}
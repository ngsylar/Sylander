using System.Collections;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public GameLogic gameLogic;

    public SlenderManAI slenderman;
    public GameObject slendermetal0;
    public GameObject slendermetal1;

    public GameObject flashlight;
    private Light lightComp;

    public AudioSource turnOn;
    public AudioSource turnOff;
    public AudioSource flickering;

    private bool on;
    private bool off;

    private bool isPaused = false;

    public float lightIntensityMin; // 0.25f
    public float lightIntensityMax; // 3f // 1f with fog

    [SerializeField, ReadOnly]
    private float lightIntensityCurrent; // 1f

    public float worldLightOff; // 0.1f
    public float worldLightMin; // 0.15f
    public float worldLightMax; // 0.5f // 0.18f with fog

    [SerializeField, ReadOnly]
    private float worldLightCurrent;

    public float fogDensityOff; // 0.1f // 0.2f with fog
    public float fogDensityMin; // 0.1f
    public float fogDensityMax; // 0.035f

    [SerializeField, ReadOnly]
    private float fogDensityCurrent;

    public float batteryMinutes;
    private float batterySeconds;
    [SerializeField] private float elapsedSeconds;

    [SerializeField]
    private float slendermanDrainFactor; // 1.75f
    public float slenderDistanceToFail; // 3f
    public float gazeFactor = 1f;

    [SerializeField, ReadOnly]
    private float currentBattery;

    private bool isFlicking = false;
    public float flickCooldown;

    [SerializeField, ReadOnly]
    private float flickTimer = 0f;

    private float batteryTimer = 0f;

    public bool IsOn
    {
        get => on;
    }

    public float CurrentBattery
    {
        get => currentBattery;
    }

    public bool IsPaused
    {
        get => isPaused;
        set => isPaused = value;
    }

    void Start()
    {
        lightComp = flashlight.GetComponent<Light>();

        off = true;
        flashlight.SetActive(false);

        SetupFlashlight();
    }

    void SetupFlashlight()
    {
        elapsedSeconds = 0f;
        batterySeconds = batteryMinutes * 60f;
        currentBattery = 1f;

        lightIntensityCurrent = lightIntensityMax;
        worldLightCurrent = worldLightMax;
        fogDensityCurrent = fogDensityMax;

        RenderSettings.ambientLight = new Color(worldLightOff, worldLightOff, worldLightOff);
        RenderSettings.fogDensity = fogDensityOff;
    }

    void Update()
    {
        if (isPaused || gameLogic.IsDead) return;

        if (off && Input.GetButtonDown("flashlight") && !isFlicking) {
            turnOn.Play();
            LightsOn();

        } else if (on) {
            DrainBattery();

            if (slenderman.DistanceToPlayer <= slenderDistanceToFail) {
                LightsOff();
                return;
            }
            if (Input.GetButtonDown("flashlight") && !isFlicking) {
                turnOff.Play();
                LightsOff();
            }
            else FlickLogic();
        }
    }

    public void Restart()
    {
        SetupFlashlight();
        LightsOn();
    }

    void LightsOn ()
    {
        if (currentBattery <= 0f && !isFlicking) {
            return;
        }
        flashlight.SetActive(true);
        off = false;
        on = true;

        slenderman.UpdateActivationRange();

        lightComp.intensity = lightIntensityCurrent;
        RenderSettings.ambientLight = new Color(worldLightCurrent, worldLightCurrent, worldLightCurrent);
        RenderSettings.fogDensity = fogDensityCurrent;
        TurnMetal1();
    }

    void LightsOff (bool playSound=true)
    {
        flashlight.SetActive(false);
        off = true;
        on = false;

        slenderman.UpdateActivationRange();

        RenderSettings.ambientLight = new Color(worldLightOff, worldLightOff, worldLightOff);
        RenderSettings.fogDensity = fogDensityOff;
        TurnMetal0();
    }

    void DrainBattery ()
    {
        float d = Mathf.InverseLerp(slenderman.InterferenceMin, slenderman.InterferenceMax, slenderman.DistanceToPlayer);
        float distanceFactor = Mathf.Lerp(1f, slendermanDrainFactor, d);
        elapsedSeconds += Time.deltaTime * distanceFactor * gazeFactor;
        float t = elapsedSeconds / batterySeconds;
        currentBattery = 1f - t;

        batteryTimer += Time.deltaTime;
        if (batteryTimer < 0.05f) return;
        batteryTimer = 0f;

        lightIntensityCurrent = Mathf.Lerp(lightIntensityMax, lightIntensityMin, t);
        worldLightCurrent = Mathf.Lerp(worldLightMax, worldLightMin, t);
        fogDensityCurrent = Mathf.Lerp(fogDensityMax, fogDensityMin, t);

        lightComp.intensity = lightIntensityCurrent;
        RenderSettings.fogDensity = fogDensityCurrent;
        Color c = RenderSettings.ambientLight;
        c.r = c.g = c.b = worldLightCurrent;
        RenderSettings.ambientLight = c;
    }

    public void ExternalFlickLogic ()
    {
        if (isFlicking)
            return;

        flickering.Play();
        StartCoroutine(FlickRoutine());

        flickTimer = 0f;
    }

    void FlickLogic ()
    {
        if (isFlicking)
            return;
        
        flickTimer += Time.deltaTime;
        if (flickTimer < flickCooldown)
            return;
        
        float flickChance = GetFlickChance();
        
        // Debug.Log(""+flickChance);
        if (Random.value < flickChance)
        {
            flickering.Play();
            StartCoroutine(FlickRoutine());
        }
        flickTimer = 0f;
    }

    IEnumerator FlickRoutine()
    {
        isFlicking = true;
        int flickCount = Random.Range(7, 9);

        for (int i=0; i < flickCount; i++) {
            if (on) LightsOff();
            else if (off) LightsOn();
            yield return new WaitForSeconds(Random.Range(0.05f, 0.10f));
        }
        // Chance de desligar completamente
        float shutdownChance = GetShutdownChance();
        if (Random.value < shutdownChance) {
            LightsOff();
            // yield return new WaitForSeconds(Random.Range(0.5f, 2f)); // Delay pra aumentar tensão
        }
        isFlicking = false;
    }

    float GetFlickChance()
    {
        if (currentBattery <= 0f) return 1f;
        if (currentBattery <= 0.1f) return 0.21f;
        if (currentBattery <= 0.2f) return 0.13f;
        if (currentBattery <= 0.5f) return 0.05f;
        if (currentBattery <= 0.8f) return 0.02f;
        return 0f;
    }

    float GetShutdownChance()
    {
        if (currentBattery <= 0f) return 1f;
        if (currentBattery <= 0.1f) return 0.8f;
        if (currentBattery <= 0.2f) return 0.5f;
        if (currentBattery <= 0.5f) return 0.3f;
        if (currentBattery <= 0.8f) return 0.1f;
        return 0f;
    }

    void TurnMetal0()
    {
        slendermetal0.SetActive(true);
        slendermetal1.SetActive(false);
    }

    void TurnMetal1()
    {
        slendermetal0.SetActive(false);
        slendermetal1.SetActive(true);
    }
}

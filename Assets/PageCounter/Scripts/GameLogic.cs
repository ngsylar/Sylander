using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // Import the TextMeshPro namespace

public class GameLogic : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds5 = new(5f);
    public Flashlight flashlight;
    public BackgroundMgmt bgMusic;

    public GameObject counter;
    public int pageCount;
    private TextMeshProUGUI counterText;

    public GameObject victoryUI;
    public GameObject deathUI;

    public GameObject staticObject;
    private VideoPlayer videoPlayer;
    public VideoClip staticVideo;       // Reference to the static video
    public VideoClip deathVideo;        // Reference to the death video
    private Renderer staticRenderer;    // Reference to the renderer of the static object
    public Material staticMaterial;     // Reference to the static material (Fade)
    public Material deathMaterial;      // Reference to the death material (Opaque)

    [SerializeField, ReadOnly] private int currentClassPriority = -1;
    private bool isDeathVideoPlaying = false; // Flag to check if death video is playing

    public bool IsDead
    {
        get => isDeathVideoPlaying;
    }

    public bool IsDeathVideoPlaying
    {
        get => isDeathVideoPlaying;
    }

    void Start()
    {
        pageCount = 0;
        counterText = counter.GetComponent<TextMeshProUGUI>();
        UpdateUI();

        StartVideoHandler();

        //Show the no escape UI
        victoryUI.SetActive(true);
        StartCoroutine(NoEscapeUI());
    }

    public void AddPage()
    {
        pageCount++;
        UpdateUI();
        bgMusic.UpdateMusic(pageCount);
    }

    void UpdateUI()
    {
        counterText.text = pageCount + "/8";
    }

    private IEnumerator NoEscapeUI(){
        yield return _waitForSeconds5;
        victoryUI.SetActive(false);
    }

    void StartVideoHandler()
    {
        // Ensure the "static" object is initially turned off
        if (staticObject != null) {
            staticObject.SetActive(false);

        // Get the VideoPlayer component from the static object
            videoPlayer = staticObject.GetComponent<VideoPlayer>();
            staticRenderer = staticObject.GetComponent<Renderer>();
            videoPlayer.clip = staticVideo; // Set the initial video clip to the static video

            // Register for the video end event
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    public void KeepStaticVideo(
        int classSlenderId,
        float interferenceMin, float interferenceMax, float distanceToPlayer,
        float staticAlphaMin, float staticAlphaMax
    ) {
        if (isDeathVideoPlaying) return;
        currentClassPriority = classSlenderId;

        if (staticObject != null && !staticObject.activeSelf) {
            staticObject.SetActive(true);
        }
        // Play the static video and reset the material and scale
        if (videoPlayer != null && videoPlayer.clip != staticVideo) {
            videoPlayer.clip = staticVideo;
            videoPlayer.SetDirectAudioMute(0, true); // Mute audio for static video
            videoPlayer.Play();

            // Reset the scale of the static object for the static video
            staticObject.transform.localScale = new Vector3(1f, 1f, 1f); // Adjust as needed
        }
        float t = Mathf.InverseLerp(interferenceMin, interferenceMax, distanceToPlayer);
        float alpha = Mathf.Lerp(staticAlphaMin, staticAlphaMax, t);

        Color color = staticMaterial.color;
        color.a = alpha;
        staticMaterial.color = color;
    }

    public void KeepDeathVideo(bool screamNoise)
    {
        // Play the death video and adjust the material and scale
        if (videoPlayer != null && videoPlayer.clip != deathVideo) {
            isDeathVideoPlaying = true;
            videoPlayer.clip = deathVideo;
            staticRenderer.material = deathMaterial;
            videoPlayer.SetDirectAudioMute(0, false); // Unmute audio
            videoPlayer.Play();

            // Adjust the scale of the static object for the death video
            staticObject.transform.localScale = new Vector3(1.28f, 0.72f, 1f);
            if (screamNoise) bgMusic.PlayScreamSound();
            flashlight.Restart();
        }
    }

    public void StopVideo(int classSlenderId)
    {
        if (classSlenderId >= 0 && currentClassPriority != classSlenderId) return;
        currentClassPriority = -1;
        
        if (!isDeathVideoPlaying && staticObject != null && staticObject.activeSelf)
            staticObject.SetActive(false);
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (vp.clip == deathVideo)
        {
            // Show the death UI when the death video ends
            deathUI.SetActive(true);

            // Restart the game
            StartCoroutine(RestartAndResetDeathUI());
        }
    }

    private IEnumerator RestartAndResetDeathUI()
    {
        // Wait for a few seconds
        yield return _waitForSeconds5; // Adjust the time as needed

        // Restart the game
        RestartGame();

        // Reset the death UI to inactive
        deathUI.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

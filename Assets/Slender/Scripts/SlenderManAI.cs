using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class SlenderManAI : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds5 = new(5f);
    
    public SlendermanChase chaser;

    public Transform player; // Reference to the player's GameObject
    public float teleportMaxDistance; // 10f // Base teleportation distance
    public float teleportMinDistance; // 8f // Has to be bigger than deathActivationRange
    public float teleportCooldown; // 12f // Base time between teleportation attempts
    public float teleportProbability; // 0.02f // Base probability of chasing the player
    public float rotationSpeed = 5f; // Rotation speed when looking at the player
    public int maxTeleportsPerChase; // 2

    private int teleportsThisChase = 0;

    public AudioClip teleportSound; // Reference to the teleport sound effect
    private AudioSource audioSource;

    public GameObject staticObject; // Reference to the "static" GameObject
    private float staticActivationRange; // Range at which "static" should be activated
    public float deathActivationRange; // 3f // Range at which death should be activated
    public VideoClip staticVideo; // Reference to the static video
    public VideoClip deathVideo; // Reference to the death video
    public Material staticMaterial; // Reference to the static material (Fade)
    public Material deathMaterial; // Reference to the death material (Opaque)

    private Vector3 baseTeleportSpot;
    private float teleportTimer = 0f;
    private int teleportTrials = 0;

    private bool isDeathVideoPlaying = false; // Flag to check if death video is playing

    private SlenderPlayerController playerController; // Reference to the player's controller
    private VideoPlayer videoPlayer;
    private Renderer staticRenderer; // Reference to the renderer of the static object
    private GameLogic gameLogic; // Reference to the game logic script
    private int lastPageCount = -1;

    public GameObject deathUI;
    public GameObject victoryUI;

    [Header("Dinamic Things")]
    public GameObject metal0;
    public GameObject metal1;
    public float staticAlphaMin;
    public float staticAlphaMax;
    public float initialStaticVolume;

    public float InterferenceMin
    {
        get => staticActivationRange;
    }

    public float InterferenceMax
    {
        get => teleportMinDistance;
    }

    public float DistanceToPlayer
    {
        get => chaser.DistanceToPlayer;
    }

    public float DistanceToChase
    {
        get => chaser.playerDetectionRadius;
        set => chaser.playerDetectionRadius = value;
    }

    public float ChaseSpeed
    {
        get => chaser.approachSpeed;
        set => chaser.approachSpeed = value;
    }

    public float ChaseSprintDuration
    {
        get => chaser.chaseSprintDuration;
        set => chaser.chaseSprintDuration = value;
    }

    public float DistanceToStop
    {
        get => chaser.stopDistance;
        set => chaser.stopDistance = value;
    }

    public bool IsChasing
    {
        get => chaser.IsChasing;
    }

    private void Start()
    {
        baseTeleportSpot = transform.position;

        UpdateActivationRange();

        // Get or add an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set the teleport sound
        audioSource.clip = teleportSound;
        audioSource.volume = 0.3f;

        // Ensure the "static" object is initially turned off
        if (staticObject != null)
        {
            staticObject.SetActive(false);
        }

        // Get reference to the player's controller
        playerController = player.GetComponent<SlenderPlayerController>();

        // Get the VideoPlayer component from the static object
        if (staticObject != null)
        {
            videoPlayer = staticObject.GetComponent<VideoPlayer>();
            staticRenderer = staticObject.GetComponent<Renderer>();
            videoPlayer.clip = staticVideo; // Set the initial video clip to the static video

            // Register for the video end event
            videoPlayer.loopPointReached += OnVideoEnd;
        }

        // Get reference to the game logic script
        gameLogic = GameObject.FindWithTag("GameLogic").GetComponent<GameLogic>();

        //Show the no escape UI
        victoryUI.SetActive(true);
        StartCoroutine(NoEscapeUI());
    }

    public void UpdateActivationRange ()
    {
        staticActivationRange = chaser.DetectionRadius;
    }

    private void Update()
    {
        if (player == null) return;

        if (isDeathVideoPlaying) {
            // Freeze the player's movement
            playerController.canMove = false;
            return;
        }

        if (gameLogic.pageCount != lastPageCount) {
            UpdateAggressiveness(gameLogic.pageCount);
            lastPageCount = gameLogic.pageCount;
        }

        // ajusta frequencia de teleporte baseado na distancia
        float adjustedCooldown = teleportCooldown;
        float proximity = Mathf.InverseLerp(teleportMaxDistance, teleportMinDistance, DistanceToPlayer);
        float distanceFactor = Mathf.Lerp(0.7f, 1.6f, proximity);
        adjustedCooldown *= distanceFactor;

        if (IsChasing) adjustedCooldown *= 1.5f;
        teleportTimer += Time.deltaTime;

        if (teleportTimer >= adjustedCooldown)
        {
            teleportTimer = 0;
            DecideTeleportAction();
        }

        RotateTowardsPlayer();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check player distance and toggle the "static" object accordingly
        if (distanceToPlayer <= deathActivationRange)
        {
            // Play the death video and adjust the material and scale
            if (videoPlayer != null && videoPlayer.clip != deathVideo)
            {
                isDeathVideoPlaying = true;
                videoPlayer.clip = deathVideo;
                staticRenderer.material = deathMaterial;
                videoPlayer.SetDirectAudioMute(0, false); // Unmute audio
                videoPlayer.Play();

                // Adjust the scale of the static object for the death video
                staticObject.transform.localScale = new Vector3(1.28f, 0.72f, 1f);
            }
        }
        else if (distanceToPlayer <= staticActivationRange)
        {
            if (staticObject != null && !staticObject.activeSelf)
            {
                staticObject.SetActive(true);
            }

            // Play the static video and reset the material and scale
            if (videoPlayer != null && videoPlayer.clip != staticVideo)
            {
                videoPlayer.clip = staticVideo;
                videoPlayer.SetDirectAudioMute(0, true); // Mute audio for static video
                videoPlayer.Play();

                // Reset the scale of the static object for the static video
                staticObject.transform.localScale = new Vector3(1f, 1f, 1f); // Adjust as needed
            }

            float t = Mathf.InverseLerp(InterferenceMin, InterferenceMax, distanceToPlayer);
            float alpha = Mathf.Lerp(staticAlphaMin, staticAlphaMax, t);

            Color color = staticMaterial.color;
            color.a = alpha;
            staticMaterial.color = color;
        }
        else
        {
            if (staticObject != null && staticObject.activeSelf)
            {
                staticObject.SetActive(false);
            }
        }
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

    private IEnumerator NoEscapeUI(){
        yield return _waitForSeconds5;
        victoryUI.SetActive(false);
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

    private void UpdateAggressiveness(int pageCount)
    {
        // Clamp de segurança
        pageCount = Mathf.Clamp(pageCount, 0, 8);

        if (pageCount == 8)
        {
            SceneManager.LoadScene(0);
            return;
        }

        // Normaliza progressão (0 → 1 entre 0 e 7 páginas)
        float t = pageCount / 7f;

        // Curva em S (mais natural psicologicamente)
        float curve = Mathf.SmoothStep(0f, 1f, t);

        // Pequena variação aleatória (evita previsibilidade)
        float rand = Random.Range(-0.5f, 0.5f);

        // --- PRESENÇA PASSIVA ---
        chaser.superSlowSpeed = Mathf.Lerp(0f, 1.5f, curve);

        // --- CHASE ---
        DistanceToChase = Mathf.Lerp(12f, 9f, curve);          // diminui levemente
        DistanceToStop  = Mathf.Lerp(10f, 13f, curve);         // sempre maior que chase

        ChaseSpeed = Mathf.Lerp(4f, 5f, curve);

        // Mantém aceleração mais natural (evita "teleporte de velocidade")
        ChaseSprintDuration = Mathf.Lerp(3f, 1.2f, curve);

        // --- TELEPORTE ---
        teleportMinDistance = Mathf.Lerp(16f, 6f, curve);
        teleportMaxDistance = Mathf.Lerp(20f, 12f, curve);

        // Pressão mais constante (menos RNG extremo)
        teleportCooldown = Mathf.Lerp(14f, 4.5f, curve) + rand;

        teleportProbability = Mathf.Lerp(0.05f, 0.8f, curve);

        // --- AJUSTES POR MARCOS IMPORTANTES ---

        if (pageCount >= 3)
        {
            // Começa a ficar mais presente
            teleportProbability += 0.05f;
        }

        if (pageCount >= 5)
        {
            // Pressão mais direta no player
            teleportMinDistance -= 1.5f;
            teleportCooldown -= 0.5f;
        }

        if (pageCount >= 7)
        {
            // Final game: mais agressivo, mas sem quebrar o jogo
            teleportProbability = Mathf.Min(teleportProbability + 0.1f, 0.85f);
            ChaseSpeed += 0.2f;
        }

        // Clamp de segurança
        teleportCooldown = Mathf.Max(3.5f, teleportCooldown);
        teleportMinDistance = Mathf.Max(6f, teleportMinDistance);

    #if UNITY_EDITOR
        Debug.Log(
            $"[Page {pageCount}] " +
            $"ChaseDist: {DistanceToChase:F1} | StopDist: {DistanceToStop:F1} | " +
            $"Speed: {ChaseSpeed:F1} | TP: {teleportMinDistance:F1}-{teleportMaxDistance:F1} | " +
            $"CD: {teleportCooldown:F1} | Prob: {teleportProbability:F2}"
        );
    #endif
    }

    private void DecideTeleportAction()
    {
        float adjustedProbability = teleportProbability;
        if (IsChasing) adjustedProbability *= 0.4f;
        
        float randomValue = Random.value;

        #if UNITY_EDITOR
        Debug.Log("Chase Decision: "+randomValue+" to "+teleportProbability);
        #endif

        chaser.SlenderController.enabled = false;

        try {
            if ((gameLogic.pageCount == 7) && (randomValue < 0.2f))
                TeleportNearPlayer(false);

            else if (randomValue <= adjustedProbability)
                TeleportNearPlayer();

            // else TeleportToBaseSpot();
        }
        finally {
            chaser.SlenderController.enabled = true;
        }
    }

    private void TeleportNearPlayer(bool near = true)
    {
        if (!IsChasing) teleportsThisChase = 0;
        else if (teleportsThisChase >= maxTeleportsPerChase)
            return;

        float radius = near 
            ? Random.Range(teleportMinDistance, teleportMaxDistance)
            : Random.Range(teleportMinDistance, 14f);

        // Normaliza proximidade (0 = longe, 1 = muito perto)
        float proximity = Mathf.InverseLerp(teleportMaxDistance, teleportMinDistance, radius);

        // Curva (deixa mais dramático)
        float bias = Mathf.SmoothStep(0f, 1f, proximity);

        // Chance de aparecer no FOV
        float appearInViewChance = Mathf.Lerp(0.2f, 0.85f, bias);

        bool inView = Random.value < appearInViewChance;

        Vector3 direction;

        if (inView)
        {
            // Mantém dentro do FOV real (60° → metade = 30°)
            float angle = Random.Range(-25f, 25f);
            direction = Quaternion.Euler(0, angle, 0) * player.forward;

            // Se estiver muito longe, puxa mais pra perto pra garantir visibilidade
            if (radius > 18f)
                radius = Random.Range(12f, 18f);
        }
        else
        {
            // Fora do FOV (laterais / atrás)
            float angle = Random.Range(70f, 180f) * (Random.value < 0.5f ? 1 : -1);
            direction = Quaternion.Euler(0, angle, 0) * player.forward;
        }

        Vector3 randomPosition = player.position + direction.normalized * radius;
        randomPosition.y = transform.position.y;

        float teleportDistance = Vector3.Distance(randomPosition, player.position);

        if ((teleportDistance < teleportMinDistance) || (IsChasing && (teleportDistance < DistanceToPlayer)))
        {
            teleportTimer = (teleportTrials < 5) ? teleportCooldown : 0;
            teleportTrials++;

            #if UNITY_EDITOR
            Debug.Log("Abort Teleportation");
            #endif
        }
        else
        {
            chaser.ResetSprint();
            transform.position = randomPosition;
            teleportTrials = 0;

            if (IsChasing) teleportsThisChase++;

            #if UNITY_EDITOR
            Debug.Log($"Teleported | Dist: {teleportDistance:F1} | InView: {inView} | Bias: {bias:F2}");
            #endif
        }
    }

    private void TeleportToBaseSpot()
    {
        if (transform.position != baseTeleportSpot) {
            transform.position = baseTeleportSpot;
            // audioSource.Play();
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f; // Ignore the vertical component

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}

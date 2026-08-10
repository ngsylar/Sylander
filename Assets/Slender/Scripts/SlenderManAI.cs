using UnityEngine;

public class SlenderManAI : MonoBehaviour
{
    private static readonly int _classSlenderId = 0;
    private static readonly WaitForSeconds _waitForSeconds5 = new(5f);
    
    public SlendermanChase chaser;
    public SlenderMirage mirage;

    public Transform player; // Reference to the player's GameObject
    public float teleportMaxDistance; // 10f // Base teleportation distance
    public float teleportMinDistance; // 8f // Has to be bigger than deathActivationRange
    public float teleportCooldown; // 12f // Base time between teleportation attempts
    public float teleportProbability; // 0.02f // Base probability of chasing the player
    public float rotationSpeed = 5f; // Rotation speed when looking at the player
    public int maxTeleportsPerChase; // 0

    [SerializeField] private float firstCooldown; // 120f
    private float adjustedCooldown;
    private int teleportsThisChase = 0;

    private float staticActivationRange; // Range at which "static" should be activated
    public float deathActivationRange; // 2f // Range at which death should be activated

    private Vector3 baseTeleportSpot;
    [SerializeField, ReadOnly] private float teleportTimer = 0f;
    private int teleportTrials = 0;

    private SlenderPlayerController playerController; // Reference to the player's controller
    private GameLogic gameLogic; // Reference to the game logic script
    [SerializeField] private int lastSafePage;
    private int lastPageCount = -1;

    [Header("Post Escape")]
    [SerializeField] private float postEscapeCooldown; // 15f
    [SerializeField, ReadOnly] private float postEscapeTimer = 0f;

    [Header("Dinamic Things")]
    public float staticAlphaMin;
    public float staticAlphaMax;

    public float InterferenceMin
    {
        get => staticActivationRange;
    }

    public float InterferenceMax
    {
        get => teleportMinDistance;
    }

    public float StaticAlphaMin
    {
        get => staticAlphaMin;
    }

    public float StaticAlphaMax
    {
        get => staticAlphaMax;
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

    public float DistanceToEscape
    {
        get => chaser.escapeDistance;
        set => chaser.escapeDistance = value;
    }

    public bool IsChasing
    {
        get => chaser.IsChasing;
    }

    private void Start()
    {
        baseTeleportSpot = transform.position;
        adjustedCooldown = firstCooldown;

        UpdateActivationRange();

        // Get reference to the player's controller
        playerController = player.GetComponent<SlenderPlayerController>();

        // Get reference to the game logic script
        gameLogic = GameObject.FindWithTag("GameLogic").GetComponent<GameLogic>();
    }

    public void UpdateActivationRange()
    {
        staticActivationRange = chaser.DetectionRadius;
    }

    private void Update()
    {
        if (player == null || gameLogic.IsPaused) return;

        // Freeze the player's movement
        if (gameLogic.IsDeathVideoPlaying) {
            playerController.canMove = false;
            return;
        }

        if (gameLogic.pageCount != lastPageCount) {
            UpdateAggressiveness(gameLogic.pageCount);
            lastPageCount = gameLogic.pageCount;
        }

        HandleTeleportation();
        RotateTowardsPlayer();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check player distance and toggle the "static" object accordingly
        if (distanceToPlayer <= deathActivationRange) {
            gameLogic.KeepDeathVideo(!chaser.IsChaseSoundPlaying);
        }
        else if (distanceToPlayer <= staticActivationRange) {
            gameLogic.KeepStaticVideo(
                _classSlenderId,
                InterferenceMin, InterferenceMax, distanceToPlayer,
                staticAlphaMin, staticAlphaMax
            );
        } else gameLogic.StopVideo(_classSlenderId);
    }

    private void UpdateAggressiveness(int pageCount)
    {
        // Clamp de segurança
        pageCount = Mathf.Clamp(pageCount, 0, 8);

        if (pageCount == 8)
        {
            // SceneManager.LoadScene(0);
            return;
        }

        // Normaliza progressão (0 → 1 entre 0 e 7 páginas)
        float t = pageCount / 7f;

        // Curva em S (mais natural psicologicamente)
        float curve = Mathf.SmoothStep(0f, 1f, t);

        // --- PRESENÇA PASSIVA ---
        chaser.superSlowSpeed = Mathf.Lerp(0f, 1.5f, curve);

        // --- TELEPORTE ---
        teleportMinDistance = Mathf.Lerp(16f, 6f, curve);
        teleportMaxDistance = Mathf.Lerp(20f, 12f, curve);

        // Pressão mais constante (menos RNG extremo)
        teleportCooldown = Mathf.Lerp(14f, 4.5f, curve);
        teleportProbability = Mathf.Lerp(0.05f, 0.8f, curve);

        // Clamp de segurança
        teleportCooldown = Mathf.Max(4f, teleportCooldown);
        teleportMinDistance = Mathf.Max(8f, teleportMinDistance);

        // --- CHASE ---
        DistanceToChase = Mathf.Lerp(12f, 9f, curve);       // diminui levemente
        // DistanceToEscape  = Mathf.Lerp(10f, 12f, curve);    // sempre maior que chase
        ChaseSpeed = Mathf.Lerp(4f, 4.2f, curve);

        // Mantém aceleração mais natural (evita "teleporte de velocidade")
        ChaseSprintDuration = Mathf.Lerp(3f, 1.5f, curve);

        // --- AJUSTES POR MARCOS IMPORTANTES ---

        if (pageCount >= 3) // Começa a ficar mais presente
            teleportProbability += 0.05f;

        if (pageCount >= 7) // Final game: mais agressivo
            teleportProbability = Mathf.Min(teleportProbability + 0.1f, 0.85f);

    #if UNITY_EDITOR
        Debug.Log(
            $"[Page {pageCount}] " +
            $"ChaseDist: {DistanceToChase:F1} | EscapeDist: {DistanceToEscape:F1} | " +
            $"Speed: {ChaseSpeed:F1} | TP: {teleportMinDistance:F1}-{teleportMaxDistance:F1} | " +
            $"CD: {teleportCooldown:F1} | Prob: {teleportProbability:F2}"
        );
    #endif
    }

    public void StartPostEscapeTeleportCooldown()
    {
        postEscapeTimer = postEscapeCooldown;
    }

    private void HandleTeleportation()
    {
        if (postEscapeTimer > 0f)
            postEscapeTimer -= Time.deltaTime;

        teleportTimer += Time.deltaTime;
        if (teleportTimer >= adjustedCooldown) {
            teleportTimer = 0;
            float rand = Random.Range(-0.5f, 0.5f); // pequena variacao evita previsibilidade
            adjustedCooldown = teleportCooldown + rand;
            DecideTeleportAction();
        }
    }

    private void DecideTeleportAction()
    {
        if (postEscapeTimer > 0f) return;

        if (IsChasing) return;
        
        float randomValue = Random.value;

        #if UNITY_EDITOR
        Debug.Log("Chase Decision: "+randomValue+" to "+teleportProbability);
        #endif

        chaser.SlenderController.enabled = false;

        try {
            if ((gameLogic.pageCount == 7) && (randomValue < 0.2f))
                TeleportNearPlayer(false);

            else if (randomValue <= teleportProbability)
                TeleportNearPlayer();

            else if (lastPageCount <= lastSafePage)
                TeleportToBaseSpot();
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
            adjustedCooldown = teleportCooldown;
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
            chaser.JumpscareHandler.ResetRealJumpscare();

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

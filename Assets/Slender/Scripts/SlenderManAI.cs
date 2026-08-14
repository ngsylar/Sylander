using System.Collections.Generic;
using UnityEngine;

public class SlenderManAI : MonoBehaviour
{
    private static readonly int _classSlenderId = 0;
    private static readonly WaitForSeconds _waitForSeconds5 = new(5f);
    
    private HouseBuilding house;
    public PresenceHunter presence;
    public SlendermanChase chaser;

    public Transform player; // Reference to the player's GameObject
    public PresenceDetector playerPresence;

    [Header("Dynamic Properties")]
    public float teleportMaxDistance; // 10f // Base teleportation distance
    public float teleportMinDistance; // 8f // Has to be bigger than deathActivationRange
    public float teleportCooldown; // 12f // Base time between teleportation attempts
    public float teleportHouseCooldown;
    public float teleportProbability; // 0.02f // Base probability of chasing the player
    public float rotationSpeed; // 50f // Rotation speed when looking at the player

    [Header("Fixed Properties")]
    [SerializeField] private float firstCooldown; // 120f
    private float adjustedCooldown;

    private float staticActivationRange; // Range at which "static" should be activated
    public float deathActivationRange; // 2f // Range at which death should be activated

    private Vector3 baseTeleportSpot;
    [SerializeField, ReadOnly] private float teleportTimer = 0f;

    private SlenderPlayerController playerController; // Reference to the player's controller
    private GameLogic gameLogic; // Reference to the game logic script
    [SerializeField] private int lastSafePage;
    private int lastPageCount = -1;

    [SerializeField] private float postEscapeCooldown; // 15f
    [SerializeField, ReadOnly] private float postEscapeTimer = 0f;

    [Header("Dynamic Environment")]
    public float staticAlphaMin;
    public float staticAlphaMax;

    public List<int> Pathfind { get; set; }

    public Vector3 GetTarget
    {
        get => (Pathfind == null || Pathfind.Count == 0)
            ? player.position : house.GetColliderByIndex(Pathfind[0]).transform.position;
    }

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

    public bool AreBothInSamePlace
    {
        get => (!playerPresence.IsInside && !presence.IsInside)
            || (playerPresence.IsPresent && presence.IsPresent);
    }

    public bool AreBothClose
    {
        get => (!playerPresence.IsInside && !presence.IsInside)
            || (playerPresence.IsInside && presence.IsInside);
    }

    public bool AreBothPresent
    {
        get => playerPresence.IsInside && presence.IsPresent;
    }

    private void Start()
    {
        house = presence.house;

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

        if (AreBothInSamePlace) HandleVideo(true);
        else HandleVideo(false);
    }

    private void HandleVideo(bool keepPlaying=true)
    {
        if (!keepPlaying || DistanceToPlayer > staticActivationRange)
            gameLogic.StopVideo(_classSlenderId);

        else if (DistanceToPlayer <= deathActivationRange && AreBothClose)
            gameLogic.KeepDeathVideo(!chaser.IsChaseSoundPlaying);

        else // if (DistanceToPlayer <= staticActivationRange)
            gameLogic.KeepStaticVideo(
                _classSlenderId,
                InterferenceMin, InterferenceMax, DistanceToPlayer,
                staticAlphaMin, staticAlphaMax
            );
    }

    private void UpdateAggressiveness(int pageCount)
    {
        // Clamp de segurança
        pageCount = Mathf.Clamp(pageCount, 0, 8);

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
        teleportHouseCooldown = Mathf.Lerp(4.5f, 14f, curve);
        teleportProbability = Mathf.Lerp(0.05f, 0.8f, curve);

        // Clamp de segurança
        teleportMinDistance = Mathf.Max(8f, teleportMinDistance);

        // --- CHASE ---
        DistanceToChase = Mathf.Lerp(12f, 9f, curve);       // diminui levemente
        // DistanceToEscape  = Mathf.Lerp(10f, 12f, curve);    // sempre maior que chase
        ChaseSpeed = Mathf.Lerp(4f, 4.2f, curve);

        // Mantém aceleração mais natural (evita "teleporte de velocidade")
        ChaseSprintDuration = Mathf.Lerp(3f, 1.5f, curve);

        // --- AJUSTES POR MARCOS IMPORTANTES ---

        if (pageCount == 1) // Acaba com a brincadeira de crianca
            teleportTimer = adjustedCooldown - teleportCooldown;

        if (pageCount >= 3) // Começa a ficar mais presente
            teleportProbability += 0.05f;

        if (pageCount >= 7) // Final game: mais agressivo
            teleportProbability = Mathf.Min(teleportProbability + 0.1f, 0.85f);

    #if UNITY_EDITOR
        Debug.Log(
            $"[Page {pageCount}] " +
            $"ChaseDist: {DistanceToChase:F1} | EscapeDist: {DistanceToEscape:F1} | " +
            $"Speed: {ChaseSpeed:F1} | TP: {teleportMinDistance:F1}-{teleportMaxDistance:F1} | " +
            $"CD: {teleportCooldown:F1} | {teleportHouseCooldown:F1} | Prob: {teleportProbability:F2}"
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
            adjustedCooldown = (AreBothPresent ? teleportHouseCooldown : teleportCooldown) + rand;
            DecideTeleportAction();
        }
    }

    private void DecideTeleportAction()
    {
        if (postEscapeTimer > 0f || IsChasing) return;
        float randomValue = Random.value;

        #if UNITY_EDITOR
        Debug.Log("Chase Decision: "+randomValue+" to "+teleportProbability);
        #endif

        chaser.SlenderController.enabled = false;

        try {
            bool shouldTeleport = randomValue <= teleportProbability;

            if (shouldTeleport) {
                if (playerPresence.IsInside) TeleportToHouse();

                else if (gameLogic.pageCount == 7 && randomValue < 0.2f)
                    TeleportNearPlayer(false);
                else TeleportNearPlayer();
            }
            else if (lastPageCount <= lastSafePage)
                TeleportToBaseSpot();
        }
        finally {
            chaser.SlenderController.enabled = true;
        }
    }

    private void TeleportNearPlayer(bool near = true)
    {
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

        if (inView) {
            // Mantem dentro do FOV real (60° → metade = 30°)
            float angle = Random.Range(-25f, 25f);
            direction = Quaternion.Euler(0, angle, 0) * player.forward;

            // Se estiver muito longe, puxa mais pra perto pra garantir visibilidade
            if (radius > 18f) radius = Random.Range(12f, 18f);
        }
        else {
            // Fora do FOV (laterais / atras)
            float angle = Random.Range(70f, 180f) * (Random.value < 0.5f ? 1 : -1);
            direction = Quaternion.Euler(0, angle, 0) * player.forward;
        }

        Vector3 randomPosition = player.position + direction.normalized * radius;
        randomPosition.y = transform.position.y;

        float teleportDistance = Vector3.Distance(randomPosition, player.position);

        if ((teleportDistance < teleportMinDistance) || (IsChasing && (teleportDistance < DistanceToPlayer))) {
            teleportTimer = AreBothPresent ? teleportHouseCooldown : teleportCooldown;
            adjustedCooldown = AreBothPresent ? teleportHouseCooldown : teleportCooldown;
            #if UNITY_EDITOR
            Debug.Log("Abort Teleportation");
            #endif
        }
        else {
            chaser.ResetSprint();
            transform.position = randomPosition;
            chaser.JumpscareHandler.ResetJumpscare();
            presence.ForceExit();
            Pathfind = null;
            #if UNITY_EDITOR
            Debug.Log($"Teleported | Dist: {teleportDistance:F1} | InView: {inView} | Bias: {bias:F2}");
            #endif
        }
    }

    private void TeleportToHouse()
    {
        int randIndex = Random.Range(0, 32);
        Collider randPlace = house.GetColliderByIndex(randIndex);
        uint playerAdjs = house.GetAdjacentsByLevel(playerPresence.CurrentPlacement, 2);

        if (house.IsIncluded(randPlace, playerAdjs)) {
            teleportTimer = AreBothPresent ? teleportHouseCooldown : teleportCooldown;
            adjustedCooldown = AreBothPresent ? teleportHouseCooldown : teleportCooldown;
            #if UNITY_EDITOR
            Debug.Log("Abort Teleportation");
            #endif
        }
        else {
            presence.ForceExit();
            chaser.ResetSprint();
            transform.position = new Vector3(
                randPlace.transform.position.x, transform.position.y,
                randPlace.transform.position.z);
            chaser.JumpscareHandler.ResetJumpscare();
            Pathfind = presence.GetRandomExplorationPath(randIndex);
            #if UNITY_EDITOR
            Debug.Log($"Teleported to the house at node #{randIndex}");
            #endif
        }
    }

    public void TeleportToBaseSpot()
    {
        if (transform.position != baseTeleportSpot)
            transform.position = baseTeleportSpot;
        presence.ForceExit();
        Pathfind = null;
    }

    private void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = GetTarget - transform.position;
        directionToPlayer.y = 0f; // Ignore the vertical component

        if (directionToPlayer != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}

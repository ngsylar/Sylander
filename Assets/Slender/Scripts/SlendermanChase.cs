using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SlendermanChase : MonoBehaviour
{
    public SlenderManAI ai;
    public AudioSource staticNoise;
    public JumpscareMgmt jumpscareMgmt;

    public float flashlightInterferenceRadius;  // 16f
    public float playerDetectionRadius;         // 12f // Radius within which the NPC detects the player
    public float approachSpeed;                 // 4.8f // Speed at which the NPC approaches the player
    public float escapeDistance;                // 12f // Distance at which the chasing NPC stops moving towards the player
    public float killDistance;                  // 1f // Distance at which the NPC kills the player
    public float chaseSprintDuration;           // 3f
    private float chaseSprintTime = 0f;
    public float superSlowSpeed; // 0.5f

    public Transform player;             // Reference to the player's transform
    public Transform playerCamera;
    public Flashlight playerFlashlight;
    [SerializeField, ReadOnly] private float distanceToPlayer = 999f;
    public Transform rayLeft;
    public Transform rayRight;

    private CharacterController controller; // Reference to the CharacterController component
    private bool isPlayerInRange = false;

    private Vector3 currentDirection;
    private bool inertia = false;
    private bool permitJumpscare = false;

    [Header("Post Escape")]
    [SerializeField] private float postEscapeSecurityTime = 3f;
    [SerializeField, ReadOnly] private float postEscapeTimer = 0f;

    public CharacterController SlenderController
    {
        get => controller;
    }

    public JumpscareMgmt JumpscareHandler
    {
        get => jumpscareMgmt;
    }

    public float DetectionRadius
    {
        get => playerFlashlight.IsOn ? flashlightInterferenceRadius : playerDetectionRadius;
    }

    public float DistanceToPlayer
    {
        get => distanceToPlayer;
    }

    public bool IsChasing
    {
        get => inertia;
    }

    public bool WasChasing
    {
        get => postEscapeTimer > 0f;
    }

    public bool IsChaseSoundPlaying
    {
        get => staticNoise.isPlaying;
    }

    public bool AreBothInSameRoom
    {
        get {
            HouseBuilding house = ai.presence.house;
            bool bothOutside = !ai.playerPresence.IsInside && !ai.presence.IsInside;
            bool bothPresent = ai.playerPresence.IsPresent && ai.presence.IsPresent;
            if (bothOutside) return true;
            Collider playerPlacement = ai.playerPresence.CurrentPlacement;
            Collider placement = ai.presence.CurrentPlacement;
            return bothOutside || (bothPresent && (
                house.frontDoor.IncludesBoth(playerPlacement, placement)
                || house.hallway.IncludesBoth(playerPlacement, placement)
                || house.betweenRooms.IncludesBoth(playerPlacement, placement)
                || house.leftRoom.IncludesBoth(playerPlacement, placement)
                || house.rightRoom.IncludesBoth(playerPlacement, placement)
                || house.backDoor.IncludesBoth(playerPlacement, placement)));
        }
    }

    public bool AreBothInDetectionRange
    {
        get { 
            HouseBuilding house = ai.presence.house;
            bool bothOutside = !ai.playerPresence.IsInside && !ai.presence.IsInside;
            bool bothInside = ai.playerPresence.IsInside && ai.presence.IsInside;
            bool nearby = distanceToPlayer <= 5f
                && house.IsIncluded(
                    ai.presence.CurrentPlacement,
                    house.GetAdjacentsByLevel(ai.playerPresence.CurrentPlacement, 2));
            return bothOutside || (bothInside && nearby);
        }
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController component is missing.");
            enabled = false;
            return;
        }

        if (player == null)
        {
            // If the player is not set in the inspector, try to find the player by tag
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
        currentDirection = transform.forward;
    }

    void Update()
    {
        if (player != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Check if the player is within the detection radius
            isPlayerInRange = distanceToPlayer <= (isPlayerInRange ? escapeDistance : playerDetectionRadius);
            isPlayerInRange = isPlayerInRange && AreBothInDetectionRange; // eh uma merda isso, dps corrijo

            // If the player is in range, move towards the player
            if (isPlayerInRange && !WasChasing) {
                MoveTowardsPlayer(distanceToPlayer);
            } else if (inertia) {
                staticNoise.Stop();
                inertia = false;
                ResetSprint();
                StartPostEscape();
            } else if (ai.AreBothPresent)
                SlowExploration();
            else SlowChase();
        }
    }

    void SlowExploration()
    {
        if (ai.Pathfind == null || ai.Pathfind.Count == 0) {
            Collider currentPlacement = ai.presence.CurrentPlacement;
            if (currentPlacement == null) return;
            ai.Pathfind = ai.presence.GetRandomExplorationPath(ai.presence.CurrentPlaceIndex);
        }
        Collider collider = ai.presence.house.GetColliderByIndex(ai.Pathfind[0]);
        Vector3 nextNode = collider.transform.position;

        if (distanceToPlayer > killDistance) {
            Vector3 direction = (nextNode - transform.position).normalized;
            Vector3 move = superSlowSpeed * Time.deltaTime * direction;
            move.y = 0;
            controller.Move(move);
            transform.LookAt(new Vector3(nextNode.x, transform.position.y, nextNode.z));
            HandlePostEscape();

            if (ai.presence.CurrentPlacement == collider)
                ai.Pathfind.RemoveAt(0);
        }
    }

    void SlowChase()
    {
        if (distanceToPlayer > killDistance) {
            Vector3 direction = (player.position - transform.position).normalized;
            Vector3 move = superSlowSpeed * Time.deltaTime * direction;
            move.y = 0;
            controller.Move(move);
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            HandlePostEscape();
        }
    }

    void HandleJumpscare()
    {
        if (!permitJumpscare || !playerFlashlight.IsOn) return;
        float jumpDistance = Mathf.Lerp(10f, 20f, playerFlashlight.CurrentBattery);
        if ((distanceToPlayer > jumpDistance) || !AreBothInSameRoom) return;

        QueryTriggerInteraction ign = QueryTriggerInteraction.Ignore;
        Vector3 src = playerCamera.position + new Vector3(0f, 0f, 4.2f);
        Vector3 l = rayLeft.position - src;
        Vector3 r = rayRight.position - src;

        if ((Physics.Raycast(src, l.normalized, out RaycastHit hitLeft, l.magnitude, ~0, ign)
            && (hitLeft.collider.gameObject == gameObject)) 
            || (Physics.Raycast(src, r.normalized, out RaycastHit hitRight, r.magnitude, ~0, ign)
            && (hitRight.collider.gameObject == gameObject))) {
            
            permitJumpscare = false;
            jumpscareMgmt.MakeJumpscare();
        }
    }

    void HandleGazeEffect()
    {
        // eu poderia usar AreBothInSameRoom aqui, mas vou dar essa colher de cha para o jogador
        if (playerFlashlight.IsOn && AreBothInDetectionRange) {
            float d = Mathf.InverseLerp(20f, 0f, distanceToPlayer);
            playerFlashlight.gazeFactor = Mathf.Lerp(1f, 1.25f, d);
        } else playerFlashlight.gazeFactor = 1f;
    }

    public void ResetSprint ()
    {
        chaseSprintTime = 0f;
    }

    void MoveTowardsPlayer(float distanceToPlayer)
    {
        #if UNITY_EDITOR
        if (!inertia) Debug.Log($"Chase started at a distance of {distanceToPlayer:F2}");
        #endif

        chaseSprintTime += Time.deltaTime;
        float t = Mathf.Clamp01(chaseSprintTime / chaseSprintDuration);
        float chaseSpeed = Mathf.Lerp(0.5f, approachSpeed, t);

        // Move towards the player if further than the stop distance
        if (distanceToPlayer > killDistance)
        {
            float inertiaFactor = 1f;
            if (inertia) {
                inertiaFactor = 3f * Time.deltaTime;
            } else {
                inertia = true;
                staticNoise.Play();
            }
            Vector3 desiredDirection = (player.position - transform.position).normalized;
            currentDirection = Vector3.Lerp(currentDirection, desiredDirection, inertiaFactor);
            Vector3 move = chaseSpeed * Time.deltaTime * currentDirection.normalized;

            // Move the NPC using the CharacterController
            controller.Move(move);

            // Adjust rotation to face the player
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
    }

    void StartPostEscape()
    {
        postEscapeTimer = postEscapeSecurityTime;
        ai.StartPostEscapeTeleportCooldown();
        if (ai.AreBothPresent)
            ai.Pathfind = ai.presence.GetRandomExplorationPath(ai.presence.CurrentPlaceIndex);
    }

    void HandlePostEscape()
    {
        if (postEscapeTimer <= 0f) return;
        postEscapeTimer -= Time.deltaTime;
    }

    // (Optional) Visualize the detection radius in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pyramid"))
            permitJumpscare = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Pyramid")) {
            HandleJumpscare();
            HandleGazeEffect();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Pyramid")) {
            playerFlashlight.gazeFactor = 1f;
            permitJumpscare = false;
        }
    }
}

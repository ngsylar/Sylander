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
    [SerializeField, ReadOnly] private bool inHouse = false;

    public Transform player;             // Reference to the player's transform
    public Flashlight playerFlashlight;
    [SerializeField, ReadOnly] private float distanceToPlayer = 999f;

    private CharacterController controller; // Reference to the CharacterController component
    private bool isPlayerInRange = false;

    private Vector3 currentDirection;
    private bool inertia = false;

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

            // If the player is in range, move towards the player
            if (isPlayerInRange && !WasChasing) {
                MoveTowardsPlayer(distanceToPlayer);
            } else if (inertia) {
                staticNoise.Stop();
                inertia = false;
                ResetSprint();
                StartPostEscape();
            } else SlowChase(distanceToPlayer);
        }
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
            Vector3 move = (inHouse ? superSlowSpeed : chaseSpeed) * Time.deltaTime * currentDirection.normalized;

            // Move the NPC using the CharacterController
            controller.Move(move);

            // Adjust rotation to face the player
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
    }

    void SlowChase(float distanceToPlayer)
    {
        if (distanceToPlayer > killDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Vector3 move = superSlowSpeed * Time.deltaTime * direction;
            controller.Move(move);
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            HandlePostEscape();
        }
    }

    void StartPostEscape()
    {
        postEscapeTimer = postEscapeSecurityTime;
        ai.StartPostEscapeTeleportCooldown();
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
        if (other.gameObject.CompareTag("Building")) {
            inHouse = true;
        }
        else if (other.gameObject.CompareTag("Pyramid")) {
            // playerFlashlight.gazeFactor = 1.25f;
            jumpscareMgmt.MakeRealJumpscare();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Building")) {
            inHouse = false;
        }
        // else if (other.gameObject.CompareTag("Pyramid"))
        //     playerFlashlight.gazeFactor = 1f;
    }
}

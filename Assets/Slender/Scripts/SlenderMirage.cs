using UnityEngine;

public class SlenderMirage : MonoBehaviour
{
    private static readonly int _classSlenderId = 1;
    private GameLogic gameLogic;
    private Collider capsuleCollider;

    public Transform player; // Reference to the player's GameObject
    [SerializeField, ReadOnly] private float distanceToPlayer = 999f;
    public float rotationSpeed = 5f; // Rotation speed when looking at the player

    public SlenderManAI slenderman;
    public SlendermanChase chaser;
    private Vector3 baseTeleportSpot;
    public float securityDistance;      // 35f
    public float teleportMaxDistance;   // 20f
    public float teleportMinDistance;   // 8f
    public float stopMusicDistance;     // 10f
    public float escapeDistance;        // 8f

    public JumpscareMgmt jumpscareMgmt;
    [SerializeField] private int maxJumpscares = 4;
    [SerializeField] private int maxJumpsPerPage = 2;
    [SerializeField, ReadOnly] private int jumpscaresDone = 0;
    [SerializeField, ReadOnly] private int jumpsPerPageDone = 0;
    [SerializeField, ReadOnly] private float jumpscareChance = 0f;
    [SerializeField, ReadOnly] private int lastJumpscarePage = -1;
    [SerializeField] private float trialCooldown = 10f;
    [SerializeField] private float jumpscareCooldown = 90f;
    [SerializeField, ReadOnly] private float currentJumpTime = 0f;
    private bool permitVideo = false;

    [SerializeField] private float hideTime = 5f; 
    [SerializeField, ReadOnly] private float currentHideTime = 0f;
    [SerializeField] private float hideVelocity;

    public float InterferenceMin
    {
        get => StaticActivationRange;
    }

    public float InterferenceMax
    {
        get => teleportMinDistance;
    }

    public float StaticActivationRange
    {
        get => chaser.DetectionRadius;
    }

    public float DistanceToPlayer
    {
        get => distanceToPlayer;
    }

    void Start()
    {
        gameLogic = GameObject.FindWithTag("GameLogic").GetComponent<GameLogic>();
        capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
        baseTeleportSpot = transform.position;
    }

    void Update()
    {
        if (gameLogic.pageCount > 4) {
            gameObject.SetActive(false);
            return;
        }
        if (player != null)
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
        RotateTowardsPlayer();

        if (chaser.DistanceToPlayer <= securityDistance) {
            TeleportToBaseSpot();
        } else if (distanceToPlayer >= securityDistance)
            JumpscareTrial();
        
        HandleStaticVideo();
        HandleHideMirage();
    }

    private void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f; // Ignore the vertical component

        if (directionToPlayer != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void JumpscareTrial()
    {
        if (jumpscaresDone >= maxJumpscares
            || gameLogic.pageCount < 1 || gameLogic.pageCount > 4
            || jumpsPerPageDone >= maxJumpsPerPage)
            return;

        currentJumpTime -= Time.deltaTime;
        if (currentJumpTime > 0f) return;
        
        float baseChance;
        float increment;

        switch (gameLogic.pageCount) {
            case 1:
                baseChance = 0.20f;
                increment = 0.04f;
                break;
            case 2:
                baseChance = 0.20f;
                increment = 0.08f;
                break;
            case 3:
                baseChance = 0.14f;
                increment = 0.06f;
                break;
            default: // pagina 4
                baseChance = 0.08f;
                increment = 0.04f;
                break;
        }
        if (jumpscareChance <= 0f)
            jumpscareChance = baseChance;

        if (Random.value <= jumpscareChance) {
            TeleportNearPlayer();
        } else {
            currentJumpTime = trialCooldown;
            jumpscareChance += increment;
            jumpscareChance = Mathf.Min(jumpscareChance + increment, 0.50f);
        }
    }

    private void TeleportNearPlayer()
    {
        float radius = Random.Range(teleportMinDistance, teleportMaxDistance);

        float angle = Random.Range(70f, 180f) * (Random.value < 0.5f ? 1 : -1);
        Vector3 direction = Quaternion.Euler(0, angle, 0) * player.forward;

        Vector3 randomPosition = player.position + direction.normalized * radius;
        randomPosition.y = transform.position.y;
        transform.position = randomPosition;

        jumpscareMgmt.Restart();
    }

    private void TeleportToBaseSpot()
    {
        if (transform.position != baseTeleportSpot) {
            if (!capsuleCollider.enabled) capsuleCollider.enabled = true;
            transform.position = baseTeleportSpot;
            gameLogic.StopVideo(_classSlenderId);
            currentHideTime = 0f;
            permitVideo = false;
        }
    }

    public void IncrementJumpscare()
    {
        jumpscaresDone++;
        jumpscareChance = 0f;

        if (lastJumpscarePage != gameLogic.pageCount) {
            currentJumpTime = jumpscareCooldown;
            jumpsPerPageDone = 0;
        }
        lastJumpscarePage = gameLogic.pageCount;
        jumpsPerPageDone++;

        permitVideo = true;
    }

    void HandleStaticVideo()
    {
        // valor se torna zero apos a ocorrencia de jumpscare
        if (jumpscareChance != 0f) {
            gameLogic.StopVideo(_classSlenderId);
            return;
        }
        if (!permitVideo) return;
        if (distanceToPlayer <= StaticActivationRange) {
            gameLogic.KeepStaticVideo(
                _classSlenderId,
                InterferenceMin, InterferenceMax, distanceToPlayer,
                slenderman.StaticAlphaMin, slenderman.StaticAlphaMax
            );
        } else gameLogic.StopVideo(_classSlenderId);
    }

    void HandleHideMirage()
    {
        if (transform.position == baseTeleportSpot || jumpscareChance != 0f)
            return;
        
        currentHideTime += Time.deltaTime;
        if (currentHideTime >= hideTime || distanceToPlayer <= escapeDistance) {
            if (capsuleCollider.enabled) capsuleCollider.enabled = false;
            transform.position -= hideVelocity * Time.deltaTime * transform.forward;
            currentHideTime = hideTime;
        }
        if (currentHideTime > 0f && distanceToPlayer >= securityDistance)
            TeleportToBaseSpot();
    }
}

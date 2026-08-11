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
    [SerializeField] private float jumpscareChance; // 0.05f
    [SerializeField] private int firstPage; // 2
    [SerializeField] private int lastPage; // 4
    [SerializeField] private float trialCooldown; // 20f
    [SerializeField] private float jumpscareCooldown; // 120f
    [SerializeField, ReadOnly] private float currentJumpTime = 0f;
    private bool permitVideo = false;

    [SerializeField] private float hideTime; // 4f 
    [SerializeField, ReadOnly] private float currentHideTime = 0f;
    [SerializeField] private float hideVelocity; // 150f

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

    public bool IsWorking
    {
        get => gameLogic.pageCount <= lastPage;
    }

    void Start()
    {
        gameLogic = GameObject.FindWithTag("GameLogic").GetComponent<GameLogic>();
        capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
        baseTeleportSpot = transform.position;
    }

    void Update()
    {
        if (gameLogic.pageCount > lastPage) {
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
        if (gameLogic.pageCount < firstPage || gameLogic.pageCount > lastPage)
            return;

        currentJumpTime -= Time.deltaTime;
        if (currentJumpTime > 0f) return;
        
        // essa condicao simploria se IsInside nao teleportar eh outra merda que deixo pra corrigir dps
        if (Random.value <= jumpscareChance && !chaser.ai.playerPresence.IsInside)
            TeleportNearPlayer();
        else currentJumpTime = trialCooldown;
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

    public void StartStaticVideo()
    {
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

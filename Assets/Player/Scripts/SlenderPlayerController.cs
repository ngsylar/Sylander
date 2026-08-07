using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SlenderPlayerController : MonoBehaviour
{
    public Camera playerCam;
    public AudioSource cameraZoomSound;
    public AudioSource breath;
    public Footsteps footsteps;

    CharacterController characterController;
    public Flashlight flashlight;
    public SlendermanChase chaser;

    public float walkSpeed; // 3f
    public float runSpeed; // 5.2f
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 75f;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public int ZoomFOV = 35;
    public int initialFOV;
    public float cameraZoomSmooth = 1;
    private bool isZoomed = false;

    [Header("Stamina")]
    public float staminaSeconds;
    public float staminaRegenSeconds;
    public float staminaCooldown;
    [SerializeField, ReadOnly] private float currentStamina;
    [SerializeField, ReadOnly] private float currentStaminaCooldown;
    private float staminaRegenerateRate;

    public bool canMove = true; // Made public
    private bool isPaused = false;

    [Header("Jumper")]
    [SerializeField] private float mapLimitX;
    [SerializeField] private float mapOffsetX;
    [SerializeField] private float mapLimitZ;
    [SerializeField] private float mapOffsetZ;

    public bool IsPaused
    {
        get => isPaused;
        set => isPaused = value;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // animator = GetComponent<Animator>();

        currentStamina = staminaSeconds;
        currentStaminaCooldown = 0f;
        staminaRegenerateRate = staminaSeconds / staminaRegenSeconds;

        #if UNITY_EDITOR
        if (flashlight.DebugMode) {
            staminaCooldown = 0.5f;
            staminaRegenSeconds = 0.25f;
            walkSpeed = 5.2f;
            runSpeed = 10f;
            staminaRegenerateRate = staminaSeconds / staminaRegenSeconds;
        }
        #endif
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isTryingToRun = Input.GetKey(KeyCode.LeftShift);
        bool isRunning = isTryingToRun && (currentStamina > 0f);
        HandleStamina(isTryingToRun, isRunning);

        float inputZ = Input.GetAxis("Vertical");
        float inputX = Input.GetAxis("Horizontal");
        Vector3 inputDirection = (forward * inputZ) + (right * inputX);

        // Normaliza só se magnitude > 1 (evita custo desnecessário)
        if (inputDirection.magnitude > 1f)
            inputDirection.Normalize();

        float speed = canMove ? (isRunning ? runSpeed : walkSpeed) : 0f;
        float movementDirectionY = moveDirection.y;
        moveDirection = inputDirection * speed;
        moveDirection.y = movementDirectionY;

        footsteps.HandleSound(inputZ, inputX, isRunning);

        moveDirection.y = movementDirectionY;
        if (!characterController.isGrounded) {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        characterController.Move(moveDirection * Time.deltaTime);

        if (!isPaused && canMove) {
            rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
        // HandleZoom();
    }

    void HandleStamina(bool isTryingToRun, bool isRunning)
    {
        if (isRunning) {
            currentStamina -= Time.deltaTime;
            if (currentStamina <= 0f) {
                currentStamina = 0f;
                currentStaminaCooldown = staminaCooldown; // só existe se zerar
                breath.Play();
                
                #if UNITY_EDITOR
                Debug.Log($"Stamina ended at a distance of {chaser.DistanceToPlayer:F2}");
                #endif
            }
        } else {
            if (currentStaminaCooldown > 0f) { // Se estiver em cooldown (só acontece se zerou)
                currentStaminaCooldown -= Time.deltaTime;
                if (currentStaminaCooldown < 0f)
                    currentStaminaCooldown = 0f;
                if (isTryingToRun) {
                    currentStaminaCooldown = staminaCooldown;
                    if (!breath.isPlaying || (breath.time >= staminaCooldown))
                        breath.Play();
                }
            } else { // Regenera sempre na mesma velocidade
                currentStamina += staminaRegenerateRate * Time.deltaTime;
                if (currentStamina > staminaSeconds)
                    currentStamina = staminaSeconds;
            }
        }
    }

    void HandleZoom()
    {
        if (Input.GetButtonDown("Fire2")) {
            isZoomed = true;
            cameraZoomSound.Play();
        }
        if (Input.GetButtonUp("Fire2")) {
            isZoomed = false;
            cameraZoomSound.Play();
        }

        if (isZoomed) {
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, ZoomFOV, Time.deltaTime * cameraZoomSmooth);
        }
        else if (!isZoomed) {
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, initialFOV, Time.deltaTime * cameraZoomSmooth);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Jumper"))
        {
            // Debug.Log(""+other.gameObject.transform.position.x+" "+other.gameObject.transform.position.z);
            if (flashlight.IsOn) flashlight.ExternalFlickLogic();
            characterController.enabled = false;

            if (other.gameObject.transform.position.x > 398) {
                float randomValue = (Random.value <= 0.05f) ? transform.position.z : Random.Range(0f, 100f);
                transform.position = new Vector3(-194f+mapLimitX+mapOffsetX, transform.position.y, randomValue);
            }
            else if (other.gameObject.transform.position.x < -199) {
                float randomValue = (Random.value <= 0.05f) ? transform.position.z : Random.Range(0f, 100f);
                transform.position = new Vector3(393f-mapLimitX+mapOffsetX, transform.position.y, randomValue);
            }
            else if (other.gameObject.transform.position.z > 197) {
                float randomValue = (Random.value <= 0.05f) ? transform.position.x : Random.Range(0f, 200f);
                transform.position = new Vector3(randomValue, transform.position.y, -94f+mapLimitZ+mapOffsetZ);
            }
            else if (other.gameObject.transform.position.z < -99) {
                float randomValue = (Random.value <= 0.05f) ? transform.position.x : Random.Range(0f, 200f);
                transform.position = new Vector3(randomValue, transform.position.y, 192f-mapLimitZ+mapOffsetZ);
            }
            characterController.enabled = true;
        }
    }
}

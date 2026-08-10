using UnityEngine;

public class CollectPages : MonoBehaviour
{
    public PageSelector pageSelector;
 
    public GameObject collectText;
    public AudioSource collectSound;

    private BoxCollider boxCollider;
    [SerializeField] private Collider placement;

    private bool inReach;
    [SerializeField, ReadOnly] private bool playerInArea;
    [SerializeField] private bool isHousePage;

    private GameObject gameLogicGO;
    private GameLogic gameLogic;

    public bool IsHousePage
    {
        get => isHousePage;
    }

    public BoxCollider GetBoxCollider
    {
        get => boxCollider;
    }

    public Collider GetPlacement
    {
        get => placement;
    }

    void Start()
    {
        collectText.SetActive(false);

        inReach = false;
        playerInArea = false;

        gameLogicGO = GameObject.FindWithTag("GameLogic");
        gameLogic = gameLogicGO.GetComponent<GameLogic>();

        boxCollider = gameObject.GetComponent<BoxCollider>();
        if (isHousePage) boxCollider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (playerInArea && other.gameObject.CompareTag("Reach")) {
            inReach = true;
            collectText.SetActive(true);
        }
        if (other.gameObject.CompareTag("Player")) {
            playerInArea = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (playerInArea && other.gameObject.CompareTag("Reach")) {
            inReach = false;
            collectText.SetActive(false);
        }
        if (other.gameObject.CompareTag("Player")) {
            playerInArea = false;
        }
    }

    void Update()
    {
        if(inReach && Input.GetButtonDown("pickup")) {
            gameLogic.AddPage();
            collectSound.Play();
            collectText.SetActive(false);
            gameObject.SetActive(false);
            inReach = false;
            pageSelector.UpdatePages(gameLogic.pageCount);
        }
        if (isHousePage && playerInArea) {
            var playerPlacement = gameLogic.playerPresence.CurrentPlacement;
            if (boxCollider.enabled) {
                if (playerPlacement != placement) {
                    boxCollider.enabled = false;
                    collectText.SetActive(false);
                    inReach = false;
                }
            } else if (playerPlacement == placement)
                boxCollider.enabled = true;
        }
    }
}

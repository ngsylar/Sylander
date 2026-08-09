using UnityEngine;

public class CollectPages : MonoBehaviour
{
    public PageSelector pageSelector;
 
    public GameObject collectText;

    public AudioSource collectSound;

    private GameObject page;

    private bool inReach;
    [SerializeField, ReadOnly] private bool playerInArea;

    private GameObject gameLogicGO;
    private GameLogic gameLogic;

    void Start()
    {
        collectText.SetActive(false);

        inReach = false;
        playerInArea = false;

        gameLogicGO = GameObject.FindWithTag("GameLogic");
        gameLogic = gameLogicGO.GetComponent<GameLogic>();

        page = this.gameObject;
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
            page.SetActive(false);
            inReach = false;
            pageSelector.UpdatePages(gameLogic.pageCount);
        }
    }
}

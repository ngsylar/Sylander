using System.Collections.Generic;
using UnityEngine;

public class DontGetLost : MonoBehaviour
{
    public GameLogic gameLogic;
    public SlenderPlayerController player;
    public Transform playerTarget;
    public Flashlight flashlight;
    public SlenderManAI slender;
    public List<Transform> transformers;
    public float timeMin = 45f;
    public float timeMax = 75f;

    [SerializeField, ReadOnly] private float chosenTime = 0f;
    [SerializeField, ReadOnly] private float timer = 0f;
    [SerializeField, ReadOnly] private bool paused = false;
    [SerializeField, ReadOnly] private bool stoped = false;

    void Start()
    {
        chosenTime = Random.Range(timeMin, timeMax);
    }

    void Update()
    {
        if (stoped || paused) return;
        if (gameLogic.pageCount == 0) return;
        timer += Time.deltaTime;
        if (slender.IsChasing) return;
        if (timer > chosenTime)
            Teleport();
    }

    private void Teleport()
    {
        slender.TeleportToBaseSpot();
        flashlight.ExternalFlickLogic();
        player.CC.enabled = false;
        try {
            float difZ = playerTarget.position.z - player.transform.position.z;
            float difX = playerTarget.position.x - player.transform.position.x;

            if (Mathf.Abs(difZ) > Mathf.Abs(difX)) {
                if (difZ < 0f) {
                    int r = Random.Range(0, 2);
                    player.transform.position = new Vector3(
                    transformers[r].position.x, player.transform.position.y, transformers[r].position.z);
                } else player.transform.position = new Vector3(
                    transformers[2].position.x, player.transform.position.y, transformers[2].position.z);
            } else {
                if (difX < 0f) player.transform.position = new Vector3(
                    transformers[3].position.x, player.transform.position.y, transformers[3].position.z);
                else player.transform.position = new Vector3(
                    transformers[4].position.x, player.transform.position.y, transformers[4].position.z);
            }
        } finally {
            player.CC.enabled = true;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player")) {
            // quem sabe depois fazer os tempos min e max diminuir de acordo com as paginas
            chosenTime = Random.Range(timeMin, timeMax);
            timer = 0f;
            stoped = true;
        }
        if (other.gameObject.CompareTag("PlayerTarget"))
            paused = true;
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Player"))
            stoped = false;
        if (other.gameObject.CompareTag("PlayerTarget"))
            paused = false;
    }
}

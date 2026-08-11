using UnityEngine;

public class BeatedLogic : MonoBehaviour
{
    public GameObject player;

    void Awake()
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.transform.SetPositionAndRotation(
            GlobalLogic.playerPosition,
            GlobalLogic.playerRotation);
        cc.enabled = true;
    }

    void Start()
    {
        enabled = false;
    }
}

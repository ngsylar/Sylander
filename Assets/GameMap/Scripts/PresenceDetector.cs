using UnityEngine;

public class PresenceDetector : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("cavalo que te ama");
    }
}

using UnityEngine;

public class SlenderMaPropsAI
{
    // private void UpdateAggressiveness(int pageCount)
    // {
    //     if (pageCount == 0)
    //     {
    //         chaser.superSlowSpeed = 0f;
    //         DistanceToChase = 12f;
    //         ChaseSpeed = 4f;
    //         ChaseSprintDuration = 4f;
    //         DistanceToStop = 20f;
    //         teleportMinDistance = 16f;
    //         teleportMaxDistance = 18f;
    //         teleportCooldown = 14f;
    //         teleportProbability = 0.05f;
    //     }
    //     else if (pageCount == 1)
    //     {
    //         chaser.superSlowSpeed = 0.5f;
    //         DistanceToChase = 10f;
    //         ChaseSpeed = 4f;
    //         ChaseSprintDuration = 3f;
    //         DistanceToStop = 20f;
    //         teleportMinDistance = 12f;
    //         teleportMaxDistance = 16f;
    //         teleportCooldown = 14f;
    //         teleportProbability = 0.1f;
    //     }
    //     else if (pageCount >= 2 && pageCount < 6)   // Gradually increase aggressiveness
    //     {
    //         float t = (pageCount - 2) / 3f;         // 0 → 1
    //         DistanceToChase = 8f - t * 2f;              // 8 -> 6
    //         ChaseSpeed = 4f + t * 0.8f;                 // 4 -> 4.8
    //         ChaseSprintDuration = 2f;
    //         DistanceToStop = 16f - t * 4f;              // 16 -> 12
    //         teleportMinDistance = 12f - t * 6f;         // 12 -> 6
    //         teleportMaxDistance = 18f - t * 4f;         // 18 -> 14
    //         teleportCooldown = 14f - t * 3f;            // 14 -> 11
    //         teleportProbability = 0.2f + t * 0.5f;      // 0.2 -> 0.7
    //     }
    //     else if (pageCount == 6)
    //     {
    //         DistanceToChase = 8f;
    //         ChaseSpeed = 4.9f;
    //         ChaseSprintDuration = 1f;
    //         DistanceToStop = 12f;
    //         teleportMinDistance = 8f;
    //         teleportMaxDistance = 12f;      // começa a aproximar
    //         teleportCooldown = 9f;          // mais pressão
    //         teleportProbability = 0.8f;     // MUITO mais agressivo, mas não 100%
    //     }
    //     else if (pageCount == 7)                // Significantly increase aggressiveness
    //     {
    //         DistanceToChase = 12f;
    //         ChaseSpeed = 5f;
    //         ChaseSprintDuration = 0.5f;
    //         DistanceToStop = 12f;
    //         teleportMinDistance = 8f;
    //         teleportMaxDistance = 12f;      // Decrease teleport distance
    //         teleportCooldown = 6.5f;        // Decrease teleport cooldown
    //         teleportProbability = 0.95f;    // Maximum catch probability
    //     }
    //     else if (pageCount == 8)                // Slenderman is defeated
    //     {
    //         SceneManager.LoadScene(0);
    //     }
    //     #if UNITY_EDITOR
    //     Debug.Log("Teleport distance: "+teleportMinDistance+"~"+teleportMaxDistance+", Probability: "+teleportProbability);
    //     #endif
    // }

    // private void TeleportNearPlayer(bool near=true)
    // {
    //     float radius = near ? Random.Range(teleportMinDistance, teleportMaxDistance) :
    //         Random.Range(teleportMinDistance, 14f);
    //     Vector3 randomPosition = player.position + Random.onUnitSphere * radius;
    //     randomPosition.y = transform.position.y; // Keep the same Y position
        
    //     float teleportDistance = Vector3.Distance(randomPosition, player.position);
    //     if (teleportDistance >= teleportMinDistance) {
    //         transform.position = randomPosition;
    //         teleportTrials = 0;
    //         // audioSource.Play();

    //         #if UNITY_EDITOR
    //         Debug.Log("Teleported with "+teleportDistance);
    //         #endif
    //     }
    //     else {
    //         teleportTimer = (teleportTrials < 5) ? 0 : teleportCooldown;
    //         teleportTrials++;

    //         #if UNITY_EDITOR
    //         Debug.Log("Abort Teleportation");
    //         #endif
    //     }
    // }
}
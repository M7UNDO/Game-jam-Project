using UnityEngine;
using StarterAssets; // Matches the namespace of your controller

public class KillBoxTrigger : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;

    [Header("Layer/Tag Setup")]
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // If the player uses your updated FirstPersonController script
            if (other.TryGetComponent<FirstPersonController>(out var fpsController))
            {
                fpsController.Teleport(respawnPoint.position, respawnPoint.rotation);
            }
            else
            {
                // Fallback for simple elements
                other.transform.position = respawnPoint.position;
                other.transform.rotation = respawnPoint.rotation;
            }
        }
    }
}
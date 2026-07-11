using UnityEngine;

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
            RespawnPlayer(other.gameObject);
        }
    }

    private void RespawnPlayer(GameObject player)
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning($"[KillBox] No respawn point assigned on {gameObject.name}! Player cannot be teleported.");
            return;
        }

        // Handle CharacterController if your player uses one (prevents physics conflicts)
        if (player.TryGetComponent<CharacterController>(out CharacterController cc))
        {
            cc.enabled = false; // Temporarily disable to allow immediate position warp
            player.transform.position = respawnPoint.position;
            player.transform.rotation = respawnPoint.rotation;
            cc.enabled = true;
            return;
        }

        // Handle Rigidbody if your player is physics-driven (zeros out falling momentum)
        if (player.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            player.transform.position = respawnPoint.position;
            player.transform.rotation = respawnPoint.rotation;

            rb.linearVelocity = Vector3.zero; // Resets falling speed so they don't keep plunging
            rb.angularVelocity = Vector3.zero;
            return;
        }

        // Default fallback for basic transforms
        player.transform.position = respawnPoint.position;
        player.transform.rotation = respawnPoint.rotation;
    }
}
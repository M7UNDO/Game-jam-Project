using UnityEngine;

public class MirrorZoneTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign the zone manager that handles this room's mirrors.")]
    [SerializeField] private MirrorZoneManager zoneManager;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the zone is the player controller
        if (other.CompareTag("Player"))
        {
            if (zoneManager != null)
            {
                zoneManager.SetMirrorsActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Turn the mirrors off the split-second the player leaves the room bounds
        if (other.CompareTag("Player"))
        {
            if (zoneManager != null)
            {
                zoneManager.SetMirrorsActive(false);
            }
        }
    }
}
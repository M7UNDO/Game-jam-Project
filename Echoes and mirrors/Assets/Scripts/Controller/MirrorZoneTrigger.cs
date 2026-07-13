using UnityEngine;

public class MirrorZoneTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign the zone manager that handles this room's mirrors.")]
    [SerializeField] private MirrorZoneManager zoneManager;

    private void OnTriggerEnter(Collider other)
    {
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
        if (other.CompareTag("Player"))
        {
            if (zoneManager != null)
            {
                zoneManager.SetMirrorsActive(false);
            }
        }
    }
}
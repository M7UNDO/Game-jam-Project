using UnityEngine;
using System.Collections.Generic;

public class MirrorZoneManager : MonoBehaviour
{
    [Header("Mirror Management")]
    [Tooltip("Drop all PlanarMirrorSurface components belonging to this specific room zone here.")]
    [SerializeField] private List<PlanarMirrorSurface> roomMirrors = new List<PlanarMirrorSurface>();

    [Header("Settings")]
    [SerializeField] private bool turnOffOnStart = true;

    private void Start()
    {
        if (turnOffOnStart)
        {
            SetMirrorsActive(false);
        }
    }

    // Call this when the player crosses the floor trigger boundary
    public void SetMirrorsActive(bool isActive)
    {
        foreach (var mirror in roomMirrors)
        {
            if (mirror != null)
            {
                mirror.enabled = isActive;

                // Optional optimization: If the mirror script doesn't completely clean up 
                // its hidden camera object when disabled, we can force the GameObject off.
                // mirror.gameObject.SetActive(isActive);
            }
        }
    }
}
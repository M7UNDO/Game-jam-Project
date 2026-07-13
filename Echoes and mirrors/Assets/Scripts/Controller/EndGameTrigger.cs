using UnityEngine;
using StarterAssets; // Matches your player controller namespace

[RequireComponent(typeof(Collider))]
public class EndGameTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("Optional UI")]
    [Tooltip("Assign your End Game Canvas or screen here to turn it on when triggered.")]
    [SerializeField] private GameObject endGameUI;

    private void Awake()
    {
        // Force the collider to be a trigger so it doesn't act as a physical wall
        if (TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            TriggerEndGame(other.gameObject);
        }
    }

    private void TriggerEndGame(GameObject player)
    {
        // 1. Stop all input processing (Look, Move, Jump, Interacting)
        if (player.TryGetComponent<FirstPersonController>(out var fpsController))
        {
            fpsController.enabled = false;
        }

        // 2. Disable the physical CharacterController to stop gravity, falling, or drift
        if (player.TryGetComponent<CharacterController>(out var characterController))
        {
            characterController.enabled = false;
        }

        // 3. Release the mouse cursor so the user can interact with menus
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 4. Activate your game over / win screen UI if you've assigned one
        if (endGameUI != null)
        {
            endGameUI.SetActive(true);
        }

        Debug.Log("End Game Triggered: Player movement and physics completely disabled.");
    }
}

using UnityEngine;

public class PuzzleDoor : MonoBehaviour, IOutput
{
    public MeshRenderer doorMesh;
    [Header("Activation Setup")]
    [SerializeField] private GameObject inputGO;
    public IInput input { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private Vector3 openOffset = new Vector3(0, 3f, 0);
    [SerializeField] private float speed = 2f;

    private Vector3 closedPosition;
    private Vector3 targetPosition;
    private Vector3 openPosition; // Storing this to make calculations cleaner

    private void Awake()
    {
        // If doorMesh isn't assigned in inspector, try to grab it
        if (doorMesh == null)
            doorMesh = gameObject.GetComponent<MeshRenderer>();

        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
        targetPosition = closedPosition;

        if (inputGO != null)
        {
            input = inputGO.GetComponent<IInput>();
            if (input != null)
            {
                RegisterToInput(input);
            }
        }
    }

    private void Update()
    {
        // Smoothly slide toward target position based on activation state
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Handle the mesh visibility based on position
        HandleMeshVisibility();
    }

    private void HandleMeshVisibility()
    {
        if (doorMesh == null) return;

        // Check if we have arrived at the closed position
        if (transform.position == closedPosition)
        {
            doorMesh.enabled = true;
        }
        // Check if we have arrived at the open position
        else if (transform.position == openPosition)
        {
            doorMesh.enabled = false;
        }
        else
        {
            // Optional: Ensure the mesh is visible while moving between states
            doorMesh.enabled = true;
        }
    }

    public void RegisterToInput(IInput inputSource)
    {
        inputSource.onTriggered += (src) => targetPosition = openPosition;
        inputSource.onUntriggered += (src) => targetPosition = closedPosition;
    }
}
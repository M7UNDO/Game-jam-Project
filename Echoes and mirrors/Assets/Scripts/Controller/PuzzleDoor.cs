using UnityEngine;

public class PuzzleDoor : MonoBehaviour, IOutput
{
    [Header("Activation Setup")]
    [SerializeField] private GameObject inputGO;
    public IInput input { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private Vector3 openOffset = new Vector3(0, 3f, 0);
    [SerializeField] private float speed = 2f;

    private Vector3 closedPosition;
    private Vector3 targetPosition;

    private void Awake()
    {
        closedPosition = transform.position;
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
    }

    public void RegisterToInput(IInput inputSource)
    {
        inputSource.onTriggered += (src) => targetPosition = closedPosition + openOffset;
        inputSource.onUntriggered += (src) => targetPosition = closedPosition;
    }
}
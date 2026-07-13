using UnityEngine;

public class PuzzleBridgeToggle : MonoBehaviour, IOutput
{
    [Header("Activation Setup")]
    [SerializeField] private GameObject inputGO;
    public IInput input { get; private set; }

    [Header("Bridge Targets")]
    [Tooltip("Assign the bridge GameObject(s) here that you want to toggle.")]
    [SerializeField] private GameObject[] bridgeObjects;

    [Tooltip("If true, the bridge starts active and turns OFF when triggered. If false, it starts hidden and turns ON.")]
    [SerializeField] private bool invertLogic = false;

    private void Awake()
    {
        // Link up to the input source component
        if (inputGO != null)
        {
            input = inputGO.GetComponent<IInput>();
            if (input != null)
            {
                RegisterToInput(input);
            }
        }

        // Set initial state on awake
        SetBridgesActive(invertLogic);
    }

    public void RegisterToInput(IInput inputSource)
    {
        // When input activates, set state based on inversion flag
        inputSource.onTriggered += (src) => SetBridgesActive(!invertLogic);

        // When input deactivates, return to the default state
        inputSource.onUntriggered += (src) => SetBridgesActive(invertLogic);
    }

    private void SetBridgesActive(bool state)
    {
        if (bridgeObjects == null) return;

        foreach (GameObject bridge in bridgeObjects)
        {
            if (bridge != null)
            {
                bridge.SetActive(state);
            }
        }
    }
}
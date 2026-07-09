using UnityEngine;

public class InteractableOutline : MonoBehaviour
{
    [SerializeField] private Outline outline;

    private void Awake()
    {
        if (outline == null)
        {
            outline = GetComponent<Outline>();
        }

        SetOutline(false);
    }

    public void SetOutline(bool enabled)
    {
        if (outline != null)
        {
            outline.enabled = enabled;
        }
    }
}
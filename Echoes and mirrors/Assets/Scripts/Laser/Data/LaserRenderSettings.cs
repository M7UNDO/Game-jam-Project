using UnityEngine;

[CreateAssetMenu(menuName = "Laser/Renderer Settings")]
public class LaserRendererSettings : ScriptableObject
{
    [SerializeField] public Color color;
    [SerializeField] public float width;
    [SerializeField, Range(1f, 200f)] public float emissionAmount;

    [Header("Build Material Fix")]
    [Tooltip("Create a new Material in your project panel using URP/Simple Lit, turn on Emission, and drop it here.")]
    [SerializeField] private Material laserMaterialTemplate;

    public void Apply(LineRenderer lineRenderer)
    {
        if (laserMaterialTemplate != null)
            lineRenderer.material = new Material(laserMaterialTemplate);
        else
            lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));

        lineRenderer.material.EnableKeyword("_EMISSION");
        lineRenderer.material.SetColor("_EmissionColor", color * emissionAmount);

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}
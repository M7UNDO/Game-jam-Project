using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Laser/Renderer Settings")]
public class LaserRendererSettings : ScriptableObject
{
    [SerializeField] public Color color;
    [SerializeField] public float width;
    [SerializeField, Range(1f, 200f)] public float emissionAmount;

    public void Apply(LineRenderer lineRenderer)
    {
        lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
        lineRenderer.material.EnableKeyword("_EMISSION");
        lineRenderer.material.SetColor("_EmissionColor", color * emissionAmount);

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}
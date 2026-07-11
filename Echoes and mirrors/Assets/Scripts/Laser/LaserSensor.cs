using System;
using System.Collections.Generic;
using UnityEngine;

public class LaserSensor : MonoBehaviour, IInput
{
    public event Action<Laser> onLaserAdded;
    public event Action<Laser> onLaserRemoved;

    public event Action<IInput> onTriggered;
    public event Action<IInput> onUntriggered;

    bool _isTriggered = false;

    public bool IsTriggered
    {
        get => _isTriggered;
        protected set
        {
            if (value == _isTriggered) return;
            _isTriggered = value;

            // Trigger the visual material swap alongside the event logic
            UpdateSensorMaterial(value);

            if (value) onTriggered?.Invoke(this);
            else onUntriggered?.Invoke(this);
        }
    }

    [Header("Visual Feedback (Multi-Material Support)")]
    [Tooltip("The material to display on slot 2 when the laser strikes the sensor.")]
    [SerializeField] private Material activeMaterial;

    private Renderer _renderer;
    private Material _originalSecondMaterial;
    private bool _hasValidSecondSlot = false;

    List<Laser> strikingLasers;

    void Awake()
    {
        strikingLasers = new List<Laser>();

        // Grab the renderer component and run defensive safety checks
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            // Unity returns a copy of the materials array. 
            // We check if it actually contains at least 2 materials (Index 0 and Index 1)
            Material[] sharedMats = _renderer.sharedMaterials;
            if (sharedMats != null && sharedMats.Length > 1)
            {
                _originalSecondMaterial = sharedMats[1];
                _hasValidSecondSlot = true;
            }
        }
    }

    void Start()
    {
        IsTriggered = false;
    }

    /// <summary>
    /// Swaps the material at slot index 1 if the conditions match perfectly.
    /// </summary>
    private void UpdateSensorMaterial(bool triggered)
    {
        // Safe exit if renderer doesn't exist or doesn't have a second material slot
        if (!_hasValidSecondSlot || _renderer == null) return;

        // Get the active instance material array copy
        Material[] currentMaterials = _renderer.materials;

        if (triggered)
        {
            if (activeMaterial != null)
            {
                currentMaterials[1] = activeMaterial;
            }
        }
        else
        {
            if (_originalSecondMaterial != null)
            {
                currentMaterials[1] = _originalSecondMaterial;
            }
        }

        // Reassign the array back to the renderer to apply changes to the mesh instance
        _renderer.materials = currentMaterials;
    }

    public static void HandleLaser(Laser laser, LaserSensor prev, LaserSensor current)
    {
        if (prev == current) return;

        if (prev != null)
        {
            prev.RemoveLaser(laser);
        }

        if (current != null)
        {
            current.AddLaser(laser);
        }
    }

    void AddLaser(Laser strikingLaser)
    {
        strikingLasers.Add(strikingLaser);
        onLaserAdded?.Invoke(strikingLaser);

        if (strikingLasers.Count == 1)
        {
            IsTriggered = true;
        }
    }

    void RemoveLaser(Laser unstrikingLaser)
    {
        strikingLasers.Remove(unstrikingLaser);
        onLaserRemoved?.Invoke(unstrikingLaser);

        if (strikingLasers.Count == 0)
        {
            IsTriggered = false;
        }
    }
}
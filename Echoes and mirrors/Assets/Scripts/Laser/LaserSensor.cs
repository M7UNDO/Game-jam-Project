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

            if (value) onTriggered?.Invoke(this);
            else onUntriggered?.Invoke(this);
        }
    }

    List<Laser> strikingLasers;

    void Awake()
    {
        strikingLasers = new List<Laser>();
    }

    void Start()
    {
        IsTriggered = false;
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
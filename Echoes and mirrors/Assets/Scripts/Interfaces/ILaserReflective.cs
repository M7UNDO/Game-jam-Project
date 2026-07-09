using UnityEngine;

public interface ILaserReflective
{
    void Reflect(Laser laser, Ray incomingRay, RaycastHit hitInfo);
}
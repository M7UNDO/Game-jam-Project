using UnityEngine;

public class LaserMirror : MonoBehaviour, ILaserReflective
{
    public void Reflect(Laser laser, Ray incomingRay, RaycastHit hitInfo)
    {
        Vector3 reflectDirection = Vector3.Reflect(incomingRay.direction, hitInfo.normal);
        Vector3 offsetOrigin = hitInfo.point + hitInfo.normal * 0.005f;
        laser.CastBeam(offsetOrigin, reflectDirection);
    }
}
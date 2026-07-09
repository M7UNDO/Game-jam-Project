using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrabbable : MonoBehaviour
{
    private Rigidbody objectRigidbody;
    private Transform objectGrabPointTransform;

    private void Awake()
    {
        objectRigidbody = GetComponent<Rigidbody>();
        
        // Ensure the rigidbody treats collisions properly during controlled movement
        objectRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        objectRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    public void Grab(Transform objectGrabPointTransform)
    {
        this.objectGrabPointTransform = objectGrabPointTransform;
        objectRigidbody.useGravity = false;
        
        // Optional: Remove any leftover momentum when grabbed
        objectRigidbody.linearVelocity = Vector3.zero;
        objectRigidbody.angularVelocity = Vector3.zero;
    }

    public void Drop()
    {
        this.objectGrabPointTransform = null;
        objectRigidbody.useGravity = true;
    }

    private void FixedUpdate()
    {
        if (objectGrabPointTransform != null)
        {
            // POSITION CONTROL: Calculate precise velocity to reach target without lagging
            Vector3 distanceToTarget = objectGrabPointTransform.position - transform.position;
            objectRigidbody.linearVelocity = distanceToTarget * 20f; // High multiplier ensures immediate snap

            // ROTATION CONTROL: Make the mirror perfectly match the grab point's angle
            // This allows the player to rotate the mirror flawlessly on the spot
            Quaternion targetRotation = objectGrabPointTransform.rotation;
            objectRigidbody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f));
        }
    }
}
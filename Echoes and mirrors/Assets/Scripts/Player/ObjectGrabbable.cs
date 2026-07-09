using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrabbable : MonoBehaviour
{
    private Rigidbody objectRigidbody;
    private Transform objectGrabPointTransform;

    [SerializeField] private Outline outline;

    private void Awake()
    {
        objectRigidbody = GetComponent<Rigidbody>();

        if (outline == null)
        {
            outline = GetComponent<Outline>();
        }

        objectRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        objectRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        SetOutline(false);
    }

    public void SetOutline(bool enabled)
    {
        if (outline != null)
        {
            outline.enabled = enabled;
        }
    }

    public void Grab(Transform objectGrabPointTransform)
    {
        this.objectGrabPointTransform = objectGrabPointTransform;
        objectRigidbody.useGravity = false;
        
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

            Vector3 distanceToTarget = objectGrabPointTransform.position - transform.position;
            objectRigidbody.linearVelocity = distanceToTarget * 20f;

            Quaternion targetRotation = objectGrabPointTransform.rotation;
            objectRigidbody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f));
        }
    }
}
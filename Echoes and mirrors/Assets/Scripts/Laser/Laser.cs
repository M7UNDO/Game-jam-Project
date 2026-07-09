using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour, IOutput
{
    bool activated = false;

    LineRenderer lineRenderer;
    [SerializeField] LaserRendererSettings laserRendererSettings;

    Vector3 sourcePosition;
    const float farDistance = 1000f;
    List<Vector3> bouncePositions;
    [SerializeField] int maxBounces = 100;
    public float spawnForwardOffset = 0.25f;

    [Header("Visual Effects Pool (Impact Hits)")]
    [SerializeField] private ParticleSystem sparkParticlePrefab;
    [SerializeField] private ParticleSystem glowingOrbPrefab;

    private List<ParticleSystem> activeSparks = new List<ParticleSystem>();
    private List<ParticleSystem> activeOrbs = new List<ParticleSystem>();

    private int currentSparkIndex = 0;
    private int currentOrbIndex = 0;

    [Header("Reflect Surface Data (Internal Tracking)")]
    private List<Vector3> hitNormals = new List<Vector3>();

    LaserSensor prevStruckLaserSensor = null;

    [SerializeField] GameObject inputGO;
    public IInput input { get; private set; }

    // Cached reference to our own collider to filter out back-reflections
    private Collider ourOwnCollider;

    void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        laserRendererSettings.Apply(lineRenderer);

        // Cache our own collider component if one exists on this object
        ourOwnCollider = GetComponent<Collider>();

        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Stretch;

        if (inputGO == null)
        {
            BehaviourIfNullInput();
            return;
        }

        input = inputGO.GetComponent<IInput>();
        if (input == null)
        {
            Debug.Log($"The input GameObject attached to {name} must contain a script which implements IInput.");
            return;
        }

        RegisterToInput(input);
    }

    void FixedUpdate()
    {
        if (!activated)
        {
            lineRenderer.positionCount = 0;
            ClearAllEffects();
            if (prevStruckLaserSensor != null)
            {
                LaserSensor.HandleLaser(this, prevStruckLaserSensor, null);
                prevStruckLaserSensor = null;
            }
            return;
        }

        sourcePosition = transform.position + transform.forward * spawnForwardOffset;
        bouncePositions = new List<Vector3>() { sourcePosition };
        hitNormals = new List<Vector3>() { Vector3.zero };

        currentSparkIndex = 0;
        currentOrbIndex = 0;

        CastBeam(sourcePosition, transform.forward);

        lineRenderer.positionCount = bouncePositions.Count;
        lineRenderer.SetPositions(bouncePositions.ToArray());

        for (int i = currentSparkIndex; i < activeSparks.Count; i++)
        {
            if (activeSparks[i].isPlaying)
            {
                activeSparks[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        for (int i = currentOrbIndex; i < activeOrbs.Count; i++)
        {
            if (activeOrbs[i].isPlaying)
            {
                activeOrbs[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    public void CastBeam(Vector3 origin, Vector3 direction)
    {
        if (bouncePositions.Count > maxBounces) return;

        Ray ray = new Ray(origin, direction);
        bool didHit = Physics.Raycast(ray, out RaycastHit hitInfo, farDistance);

        if (!didHit)
        {
            Vector3 endPoint = origin + direction * farDistance;
            bouncePositions.Add(endPoint);
            hitNormals.Add(Vector3.zero);

            if (prevStruckLaserSensor != null)
            {
                LaserSensor.HandleLaser(this, prevStruckLaserSensor, null);
                prevStruckLaserSensor = null;
            }
            return;
        }

        // --- NEW SAFETY FILTER ---
        // If the beam bounces back and hits the laser box's own collider, stop the sequence cleanly without sparking
        if (ourOwnCollider != null && hitInfo.collider == ourOwnCollider)
        {
            // Terminate the beam line right at the face of the gun muzzle smoothly
            bouncePositions.Add(hitInfo.point);
            hitNormals.Add(hitInfo.normal);

            if (prevStruckLaserSensor != null)
            {
                LaserSensor.HandleLaser(this, prevStruckLaserSensor, null);
                prevStruckLaserSensor = null;
            }
            return;
        }

        bouncePositions.Add(hitInfo.point);
        hitNormals.Add(hitInfo.normal);

        SpawnEffectsAtPoint(hitInfo.point, direction, hitInfo.normal);

        var reflectiveObject = hitInfo.collider.GetComponent<ILaserReflective>();
        if (reflectiveObject != null)
        {
            reflectiveObject.Reflect(this, ray, hitInfo);
        }
        else
        {
            var currentLaserSensor = hitInfo.collider.GetComponent<LaserSensor>();
            if (currentLaserSensor != prevStruckLaserSensor)
            {
                LaserSensor.HandleLaser(this, prevStruckLaserSensor, currentLaserSensor);
                prevStruckLaserSensor = currentLaserSensor;
            }
        }
    }

    private void SpawnEffectsAtPoint(Vector3 point, Vector3 beamDirection, Vector3 surfaceNormal)
    {
        if (sparkParticlePrefab != null)
        {
            if (currentSparkIndex >= activeSparks.Count)
            {
                ParticleSystem newSpark = Instantiate(sparkParticlePrefab, this.transform);
                activeSparks.Add(newSpark);
            }

            ParticleSystem spark = activeSparks[currentSparkIndex];
            spark.transform.position = point;
            spark.transform.rotation = Quaternion.LookRotation(beamDirection);

            if (!spark.isPlaying)
            {
                spark.Play();
            }

            currentSparkIndex++;
        }

        if (glowingOrbPrefab != null)
        {
            if (currentOrbIndex >= activeOrbs.Count)
            {
                ParticleSystem newOrb = Instantiate(glowingOrbPrefab, this.transform);
                activeOrbs.Add(newOrb);
            }

            ParticleSystem orb = activeOrbs[currentOrbIndex];
            orb.transform.position = point;
            orb.transform.rotation = Quaternion.LookRotation(surfaceNormal);

            if (!orb.isPlaying)
            {
                orb.Play();
            }

            currentOrbIndex++;
        }
    }

    private void ClearAllEffects()
    {
        foreach (var spark in activeSparks)
        {
            if (spark != null && spark.isPlaying)
            {
                spark.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        foreach (var orb in activeOrbs)
        {
            if (orb != null && orb.isPlaying)
            {
                orb.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    public void RegisterToInput(IInput inputSource)
    {
        inputSource.onTriggered += (src) => activated = true;
        inputSource.onUntriggered += (src) => activated = false;
    }

    private void BehaviourIfNullInput()
    {
        activated = true;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidsBody : MonoBehaviour
{
    private const float G = 5000f;
    public GameObject[] body;
    public BodyProperty[] bp;
    private int numberOfSphere = 200;
    TrailRenderer trailRenderer;

    [Range(0, 100)]
    public float pushForce;

    [Range(0, 100)]
    public float pushDistance;

    [Range(0, 5)]
    public float maxAcceleration;

    [Range(-50, 50)]
    public float originX;

    // New damping and cohesion variables
    [Range(0, 1)]
    public float dampingFactor = 0.99f;  // Damping to reduce velocity

    [Range(0, 10)]
    public float cohesionFactor = 2f;    // Factor to control cohesion

    // Toggles for movement direction
    public bool moveTowardsOrigin = true;  // Toggle to move towards the origin
    public bool moveAwayFromOrigin = true;  // Toggle to move away from the origin
    public List<Transform> boids;

    public struct BodyProperty
    {
        public float mass;
        public Vector3 velocity;
        public Vector3 acceleration;
    }

    void Start()
    {
        bp = new BodyProperty[numberOfSphere];
        body = new GameObject[numberOfSphere];
        pushForce = 3;
        pushDistance = 3;

        for (int i = 0; i < numberOfSphere; i++)
        {
            body[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            // initial conditions
            float r = 50f;
            body[i].transform.position = new Vector3(r * Mathf.Cos(Mathf.PI * 2 / numberOfSphere * i), r * Mathf.Sin(Mathf.PI * 2 / numberOfSphere * i), Random.Range(100, 200));
            bp[i].velocity = new Vector3(r / 10 * Mathf.Sin(Mathf.PI * 2 / 3 * i), r / 10 * Mathf.Cos(Mathf.PI * 2 / 3 * i), 0);
            bp[i].mass = 1;

            // trail
            trailRenderer = body[i].AddComponent<TrailRenderer>();
            // Configure the TrailRenderer's properties
            trailRenderer.time = 100.0f;  // Duration of the trail
            trailRenderer.startWidth = 0.5f;  // Width of the trail at the start
            trailRenderer.endWidth = 0.1f;    // Width of the trail at the end
            // a material to the trail
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            // Set the trail color over time
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(0.5f, 0.0f, 1.0f), 0.80f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trailRenderer.colorGradient = gradient;
        }
    }

    void Update()
    {
        Vector3 origin = new Vector3(originX, 0, 0);  // Origin point

        for (int i = 0; i < numberOfSphere; i++)
        {
            bp[i].acceleration = Vector3.zero;  // Reset acceleration for each frame
        }

        for (int i = 0; i < numberOfSphere; i++)
        {
            for (int j = i + 1; j < numberOfSphere; j++)
            {
                Vector3 distance = body[j].transform.position - body[i].transform.position;
                float m1 = bp[i].mass;
                float m2 = bp[j].mass;

                Vector3 gravity = CalculateGravity(distance, m1, m2);

                if (distance.sqrMagnitude > pushDistance)
                {
                    bp[i].acceleration += gravity / m1 * cohesionFactor;
                    bp[j].acceleration -= gravity / m2 * cohesionFactor;
                }
                else
                {
                    bp[i].acceleration -= gravity / m1 * pushForce;
                    bp[j].acceleration += gravity / m2 * pushForce;
                }
            }

            Vector3 originTowards = origin - body[i].transform.position;
            float distanceToOrigin = originTowards.magnitude;

            // Modify movement logic based on toggles
            if (moveTowardsOrigin)
            {
                // Move towards the origin with some damping based on distance
                bp[i].acceleration -= originTowards.normalized * Mathf.Clamp(2f / distanceToOrigin, 0.5f, 2f);
            }

            if (moveAwayFromOrigin && distanceToOrigin < 5f)  // Move away if too close
            {
                // Move away from the origin slightly if too close
                bp[i].acceleration += (body[i].transform.position - origin).normalized * pushForce * 2f;
            }
            else if (moveAwayFromOrigin)
            {
                // Allow movement away from the origin in general
                bp[i].acceleration += (body[i].transform.position - origin).normalized * 0.5f; // Add some force to move away
            }

            // Cap the acceleration to avoid extreme speeds
            if (bp[i].acceleration.magnitude > maxAcceleration)
            {
                bp[i].acceleration = bp[i].acceleration.normalized * maxAcceleration;
            }

            // Update velocity and damping
            bp[i].velocity += bp[i].acceleration * Time.deltaTime;
            bp[i].velocity *= dampingFactor;

            // Update position
            body[i].transform.position += bp[i].velocity * Time.deltaTime;
        }

        Sound sound = GetComponent<Sound>();
        sound.CalculateDistance(boids);
    }

    private Vector3 CalculateGravity(Vector3 distanceVector, float m1, float m2)
    {
        Vector3 gravity;  // note this is also Vector3
        gravity = G * m1 * m2 / (distanceVector.sqrMagnitude) * distanceVector.normalized;
        return gravity;
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    [SerializeField]
    Transform liftTransform;
    [SerializeField]
    Transform cameraTransform;
    [SerializeField]
    Transform windTransform;
    [SerializeField]
    ParticleSystem windParticles;
    [SerializeField]
    [Range(0,1)]
    float lerpRate;
    [SerializeField]
    [Range(0, 30)]
    float liftConstant;
    [SerializeField]
    [Range(0, 1)]
    float dragConstant;
    [SerializeField]
    [Range(0, 1)]
    float yawConstant;


    Rigidbody rb;
    Quaternion normal;
    float windDistance;




    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();

        // Initial normal is same as lift vector
        normal = liftTransform.rotation;
        windDistance = windTransform.localPosition.magnitude;
	}
	

	void FixedUpdate () {
        Vector3 velocity = rb.velocity;
        normal = Quaternion.Lerp(normal, liftTransform.rotation, lerpRate);
        
        // Lift
        Vector3 lift = normal * Vector3.forward;
        lift *= velocity.sqrMagnitude * LiftCoefficient() / 2;
        rb.AddForce(lift);

        // Drag
        Vector3 drag = velocity.sqrMagnitude * DragCoefficient() * - velocity.normalized;
        rb.AddForce(drag);

        // Yaw
        Vector3 yaw = velocity.sqrMagnitude * YawCoefficient() * -(normal * Vector3.right);
        rb.AddForce(yaw);

        // Wind
        // GameObject Transform
        Quaternion windRotation = Quaternion.LookRotation(velocity);
        Vector3 windLocation = velocity.normalized * (windDistance + velocity.magnitude * 0.2f);
        windTransform.SetPositionAndRotation(windLocation + transform.position, windRotation);
        // Particle control
        // Velocity
        ParticleSystem.VelocityOverLifetimeModule vel = windParticles.velocityOverLifetime;
        vel.speedModifier = new ParticleSystem.MinMaxCurve(velocity.magnitude);
        // Emission rate. Emits if velocity > 23
        ParticleSystem.EmissionModule em = windParticles.emission;
        em.rateOverTime = velocity.magnitude > 23 ? (velocity.magnitude - 23) * 2 : 0;
    }

    // Calculates the 'lift coefficient' 
    float LiftCoefficient()
    {
        float Cl = 0;
        Cl = -Vector3.Dot(rb.velocity.normalized, normal * Vector3.forward) * liftConstant;
        return Cl;
    }

    float DragCoefficient()
    {
        float Cd = 0;
        Cd = (0.2f + 
            0.8f * Mathf.Abs(Vector3.Dot(rb.velocity.normalized, normal * Vector3.down))
            )
            * dragConstant;
        return Cd;
    }

    float YawCoefficient()
    {
        float Cy = 0;
        Cy = Vector3.Dot(rb.velocity.normalized, normal * Vector3.right) * yawConstant;
        return Cy;
    }
}

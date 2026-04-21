using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPhysics : MonoBehaviour
{
    [SerializeField] ParticleSystem leftSkid;
    [SerializeField] ParticleSystem rightSkid;

    [SerializeField] Transform centerOfGravity;

    [SerializeField] AudioClip crash1;
    [SerializeField] AudioClip crash2;

    [SerializeField] AudioSource engineIdle;
    [SerializeField] AudioSource engineRun;
    [SerializeField] AudioSource wheelsSkid;
    [SerializeField] AudioSource crashSource;

    [SerializeField] AnimationCurve engineVolume;
    [SerializeField] AnimationCurve enginePitch;
    [SerializeField] AnimationCurve engineForceCurve;

    [SerializeField] Rigidbody rb;
    [SerializeField] Transform[] wheels;
    [SerializeField] Transform[] suspensionLocalOrigin;
    [SerializeField] float suspensionFriction;
    [SerializeField] float sideWheelFriction = 2400;
    [SerializeField] float wheelRadius = 0.48f;
    [Range(0.0f,0.5f)]
    [SerializeField] float suspensionRange= 0.4f;

    [Range(0.0f, 1.0f)]
    [SerializeField] float thrust = 0.0f;
    [Range(-1.0f, 1.0f)]
    [SerializeField] float steering = 0.0f;
    [Range(-45.0f, 45.0f)]
    [SerializeField] float steeringAngle = 22.5f;

    public float carSpeed;
    public bool DrawGizmos = false;
    public bool userControl= false;
    // Start is called before the first frame update
    float averageForce;

    int suspensionMask;
    void Start()
    {
        averageForce = (rb.mass*Physics.gravity.magnitude)/4;
        rb.centerOfMass = centerOfGravity.localPosition;

        suspensionMask = LayerMask.GetMask("Circuit");
    }

    public bool gamepad=true;
    // Update is called once per frame
    void Update()
    {
        if (userControl)
        {
            if (!gamepad)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    thrust = 1f;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    thrust = -1f;
                }
                else if (Input.GetKey(KeyCode.W))
                {
                    thrust = 0.5f;
                }
                else
                {
                    thrust = 0;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    steering = -1;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    steering = 1;
                }
                else
                {
                    steering = 0;
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    brake = 1;
                }
                else
                {
                    brake = 0;
                }
            }
            else
            {
                steering = Input.GetAxisRaw("Horizontal");
                thrust= Input.GetAxisRaw("Vertical");
            }
        }
    }

    private void FixedUpdate()
    {
        for(int c = 0; c < 2; c++)
        {
            suspensionLocalOrigin[c].localRotation = Quaternion.Euler(0, (steering * steeringAngle), 0);
        }

        carSpeed = Vector3.Project(rb.linearVelocity,transform.forward).magnitude;
        float gear = 0.60f + Mathf.Repeat(carSpeed, 8)*0.07f;
        engineRun.pitch = gear;

        float ev = engineVolume.Evaluate(Mathf.Max(Mathf.Abs(thrust), carSpeed*0.02f));
        engineIdle.volume = (1 - ev)*0.6f;
        engineRun.volume = ev*0.6f;

        for (int c = 0; c < wheels.Length; c++)
        {
            HandleSuspension(wheels[c], suspensionLocalOrigin[c],c<2);
        }

    }

    public float engineForce = 45000;

    public float distance;

    float backWheelsAngle = 0;


    public void HandleSuspension(Transform wheel,Transform suspensionOrigin,bool forwardTires)
    {

        //Side friction
        RaycastHit rch;
        bool hit = Physics.Raycast(suspensionOrigin.position,-suspensionOrigin.up,out rch,suspensionRange*2+wheelRadius, suspensionMask, QueryTriggerInteraction.Ignore);
        if (hit)
        {


            wheel.position = rch.point + suspensionOrigin.up * wheelRadius;
            float d = Vector3.Distance(wheel.position,suspensionOrigin.position) / (suspensionRange * 2);
            float direction = Vector3.Dot(suspensionOrigin.up,suspensionOrigin.position-wheel.position)>0?1f:-1f;
            distance = d *direction<0?1:1-d;

            Vector3 vDampen = Vector3.Project(rb.GetPointVelocity(wheel.position), suspensionOrigin.up);
            float fDampen= vDampen.magnitude*suspensionFriction;
            vDampen = vDampen.normalized* Mathf.Pow(averageForce*fDampen,1f);

            Vector3 force = (averageForce *((distance-0.5f)*4+0.5f) )*suspensionOrigin.up*2;
            rb.AddForceAtPosition(force-vDampen,wheel.position);

            if (!forwardTires)
            {
                 Vector3 wv = rb.GetPointVelocity(wheel.position-suspensionOrigin.up*wheelRadius);
                float floorSpeed = (Vector3.Project(wv, suspensionOrigin.forward).magnitude) * ((Vector3.Dot(transform.forward, wv) > 0) ? 1f : -1f);
                backWheelsAngle += floorSpeed *Mathf.PI*0.25f*wheelRadius;
                wheel.localRotation = Quaternion.Euler(backWheelsAngle,0,0);
            }
            else
            {
                
                Vector3 wv = rb.GetPointVelocity(wheel.position - suspensionOrigin.up * wheelRadius);
                float floorSpeed = (Vector3.Project(wv, suspensionOrigin.forward).magnitude) * ((Vector3.Dot(transform.forward, wv) > 0) ? 1f : -1f);

                Vector3 vforce = (Quaternion.Euler(0, steeringAngle * steering, 0) * suspensionOrigin.forward) * engineForce;
                rb.AddForceAtPosition(vforce * thrust*engineForceCurve.Evaluate(Mathf.Abs(floorSpeed/20)), wheel.position);

                backWheelsAngle += floorSpeed * Mathf.PI * 0.25f * wheelRadius;
                wheel.localRotation = Quaternion.Euler(backWheelsAngle, 0, 0);
            }
            

            //Side friction
            Vector3 sv = rb.GetPointVelocity(wheel.position);

            float brakeMagnitude = (Vector3.Project(sv, suspensionOrigin.forward) * brake * brakeFriction).magnitude;

            Vector3 brakeVector = Vector3.ClampMagnitude(Vector3.Project(sv, suspensionOrigin.forward) * brake * brakeFriction, maxBrakeForce);

            if (!forwardTires&& ((Vector3.Project(sv, wheel.right) * (averageForce)).magnitude>sideWheelFriction||brakeMagnitude>maxBrakeForce))
            {
                wheelsSkid.volume = Mathf.Lerp(wheelsSkid.volume, 1,Time.deltaTime*4);
                if (!leftSkid.isPlaying&& !rightSkid.isPlaying)
                {
                    leftSkid.Play(true);
                    rightSkid.Play(true);
                }
                wheelsSkid.pitch =0.8f+Mathf.Pow((sv.magnitude*0.022f),2);
            }
            else
            {
                
                wheelsSkid.volume = Mathf.Lerp(wheelsSkid.volume, 0, Time.deltaTime*4);
                

                if (!forwardTires && (Vector3.Project(sv, wheel.right) * (averageForce)).magnitude < sideWheelFriction && (brakeMagnitude < maxBrakeForce))
                {
                    if (leftSkid.isPlaying && rightSkid.isPlaying)
                    {
                        leftSkid.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        rightSkid.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    }
                }

            }


            Vector3 sideFriction = Vector3.ClampMagnitude(Vector3.Project(sv,wheel.right)*(averageForce),sideWheelFriction);
            rb.AddForceAtPosition(-sideFriction-brakeVector, wheel.position);
        }
        else
        {
            wheel.position = Vector3.Lerp(wheel.position, suspensionOrigin.position - suspensionOrigin.up * (suspensionRange * 2), Time.fixedDeltaTime * 2);
            float d = Vector3.Distance(wheel.position, suspensionOrigin.position) / (suspensionRange * 2);
            float direction = Vector3.Dot(suspensionOrigin.up, suspensionOrigin.position - wheel.position) > 0 ? 1f : -1f;
            distance = d * direction < 0 ? 1 : 1 - d;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 3)
        {
            crashSource.PlayOneShot(Random.value>0.5f?crash1:crash2);
        }

        transform.GetChild(0).GetComponent<NN_Sensor>().OnCollisionEnter(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        transform.GetChild(0).GetComponent<NN_Sensor>().OnCollisionStay(collision);
    }
    private void OnDrawGizmos()
    {
        if (DrawGizmos)
        {
            Gizmos.color=new Color(1,1,1,0.35f);
            foreach (Transform t in wheels)
            {
                Gizmos.DrawSphere(t.position, wheelRadius);
            }
        }
    }

    public void SetSteering(float s)
    {
        steering = Mathf.Clamp(s,-1f,1f);
    }

    public void SetThrust(float t)
    {
        thrust= Mathf.Clamp(t, -1f, 1f);
    }


    public float brakeFriction = 200f;
    public float maxBrakeForce =200f;
    float brake=0;
    public void SetBrake(float t)
    {
        brake = Mathf.Clamp01(t);
    }

}

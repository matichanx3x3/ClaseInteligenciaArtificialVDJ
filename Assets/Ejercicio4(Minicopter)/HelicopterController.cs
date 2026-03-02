using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterController : MonoBehaviour
{
    public Transform TakeOffZone;
    public Transform LandingZone;

    float stationaryForce;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        stationaryForce = (rb.mass * Physics.gravity).magnitude;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Control();
        AnimateHelix();
    }


    [Range(0f,1f)]public float rotorForce = 0.5f;
    public float yawForce=0;
    public float rollForce = 0;
    [Range(-1f,1f)]public float pitchForce = 0;
    public bool isObstacleInPath;
    public string obstacleName;
    void Control()
    {
        //SetRotorForce(rotorForce);
        //SetYawForce(yawForce);
        //SetRollForce(-rollForce);

        isObstacleInPath = AreObstaclesInPath();

        float alturaMax = 32;
        float dElev = alturaMax - transform.position.y; // diferencia, si es positiva tiene que subir
        Debug.Log("altura actual" + dElev);
        float vElev = rb.linearVelocity.y;
        float balance = -vElev;

        SetRotorForce((dElev+balance)+0.5f);
        //SetPitchForce(pitchForce);
        //0.5 es la potencia exacta para mantener el helicoptero estatico

        //calcular la distancia hasta que se desea subir para que de esta forma el helicoptero
        //logre sortear el obstaculo.
        //la idea general es pensar hasta donde tiene que subir para sortear el obstaculo
        //una vez que se este en la altura que se busca, inclinar ligeramente el helicoptero para 
        //luego usar setpitchforce
        //cuando se llegue el objetivo, se tiene que contrarestar el pitchforce con un pitch force negativo
        //y cuando se llegue abajo se tiene que aterrizar ligeramente.

    }

    void SetRotorForce(float input) //From 0 to 1 controls heigh
    {
        float rf=Mathf.Clamp01(input);
        rb.AddForce(transform.forward*stationaryForce*2*rf);
    }

    void SetPitchForce(float input) //From -1 to 1 controls forward or backward
    {
        float rf = Mathf.Clamp(input,-1,1);
        rb.AddRelativeTorque(new Vector3(rf * 200, 0, 0));
    }

    void SetYawForce(float input) //From -1 to 1 controls turning left or right
    {

        float rf = Mathf.Clamp(input, -1, 1);
        rb.AddRelativeTorque(new Vector3(0, 0, rf * 200));
    }

    void SetRollForce(float input) //From -1 to 1 do a barrel roll
    {
        float rf = Mathf.Clamp(input, -1, 1);
        rb.AddRelativeTorque(new Vector3(0, rf * 200, 0));
    }

    void AnimateHelix()
    {
        transform.GetChild(0).Rotate(Vector3.forward, 360 * Time.fixedDeltaTime*4);
    }

    bool AreObstaclesInPath()
    {
        RaycastHit rch;
        bool hit = Physics.SphereCast(transform.position,4,LandingZone.position-transform.position,out rch);

        if (hit&&rch.transform == LandingZone)
        {
            obstacleName = rch.transform.name;
            return false;
        }
        else if (hit)
        {
            obstacleName = rch.transform.name;
            return true;
        }
        else
        {
            obstacleName = "";
            return true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 crashSpeed = collision.relativeVelocity;

        Debug.Log(crashSpeed.magnitude);
        if (crashSpeed.magnitude>13)
        {
            for (int c = 0; c < transform.childCount; c++)
            {
                transform.GetChild(c).gameObject.SetActive(true);
                
                transform.GetChild(c).GetComponent<Rigidbody>().linearVelocity = -crashSpeed;
                transform.GetChild(c).GetComponent<Rigidbody>().isKinematic = false;
                transform.GetChild(c).parent = null;
            }

            Destroy(gameObject);
        }
    }
}

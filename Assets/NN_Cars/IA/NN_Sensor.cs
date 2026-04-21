using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NN_Sensor : MonoBehaviour
{

    [SerializeField] public NN_Neuron[] inputs;
    [SerializeField] public NN_Neuron[] outputs;
    [SerializeField] public CarPhysics carController;


    [SerializeField] int lm;


    [SerializeField] public float score;
    [SerializeField] public float hitPenalty= 1;
    [SerializeField] public float frictionPenalty = 0.5f;
    [SerializeField] public float collisions;
    private float timerForNewCollision = 0.5f;
    private bool colBool = false;
    private void Start()
    {
        lm=LayerMask.GetMask("Circuit");
        score = 0;
        collisions = 0;
        //brakeTime = 0;
    }

    void Update()
    {
        if (colBool)
        {
            timerForNewCollision -= Time.deltaTime;
            if (timerForNewCollision <= 0)
            {
                colBool = false;
                timerForNewCollision = 0.5f;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        /*Set neural network input*/
        float[] distances = new float[inputs.Length];
        for(int c =0;c<inputs.Length;c++)
        {
            distances[c] = 1;
            RaycastHit rch;
            bool hit = Physics.Raycast(transform.position,transform.forward+(transform.right*(c-4.0f+0.5f)),out rch,10, lm, QueryTriggerInteraction.Ignore);
            if (hit)
            {
                distances[c] = rch.distance / 10;
            }
            inputs[c].SetInput(distances[c]);
        }


        /*Evaluate output*/

        carController.SetThrust(outputs[0].GetOutput());
        carController.SetBrake(outputs[1].GetOutput());
        carController.SetSteering(outputs[2].GetOutput());


        /*Compute grade*/
        CalculateScore();
    }

    bool hit = false;
    public void CalculateScore()
    {
        //aquellos que van más rapido tienen más nota.
        //si no se chocan se les da un poquito más de nota.
        if(!hit && carController.carSpeed > 1)
            score+= Time.fixedDeltaTime;
        else if (hit && carController.carSpeed > 1)
            score+= Time.fixedDeltaTime - 1;
    }

    public float GetScore()
    {
        return score;
    }

    public void OnCollisionStay(Collision collision)
    {
        if(collision.collider.tag != "coxecito" && carController.carSpeed > 1)
            score-= frictionPenalty*0.0015f;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (!colBool && carController.carSpeed > 1)
        {
            collisions++;
            score -= hitPenalty;
            colBool = true;
        }
    }

}

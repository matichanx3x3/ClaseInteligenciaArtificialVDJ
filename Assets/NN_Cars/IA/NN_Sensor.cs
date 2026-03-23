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
    [SerializeField] public float hitPenalty=25;
    [SerializeField] public float frictionPenalty = 25;
    [SerializeField] public float collisions;
    private void Start()
    {
        lm=LayerMask.GetMask("Circuit");
        score = 0;
        collisions = 0;
        //brakeTime = 0;
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
        //si se chocan se les da un poquito más de nota.

        if(!hit)
            score+= Time.fixedDeltaTime;
    }

    public float GetScore()
    {
        return score;
    }

    private void OnCollisionStay(Collision collision)
    {

    }

    public void OnCollisionEnter(Collision collision)
    {
     
    }




}

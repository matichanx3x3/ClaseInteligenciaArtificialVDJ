using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NN_Neuron:MonoBehaviour
{

    public enum RNNFunction { Identity, SoftPlus, ReLu, TanH, Sigmoid,Input };
    [SerializeField] public RNNFunction activationFunction;
    [SerializeField] public NN_Neuron[] connections;
    [SerializeField] public float[] weights;

    /*public void AddConnection(NN_Neuron connectTo,float weight)
    {
        NN_Neuron[] temp = new NN_Neuron[connections.Length+1];
        connections.CopyTo(temp, 0);
        connections = temp;

        float[] tempW = new float[connections.Length + 1];
        weights.CopyTo(tempW, 0);
        weights = tempW;
    }*/

    public void SetConnections(NN_Neuron[] conns,float[] ws)
    {
        float[] tweights = ws;
        if (tweights == null)
        {
            tweights = new float[conns.Length];
        }

        connections = (NN_Neuron[])conns.Clone();
        weights = (float[])tweights.Clone();
    }


    public float GetOutput()
    {
        if (activationFunction == RNNFunction.Input)
        {
            return input;
        }
        float value = 0;
        for(int c=0;c<connections.Length;c++)
        {
            value += connections[c].GetOutput() * weights[c];
        }
        float output;
        switch (activationFunction)
        {
            case RNNFunction.SoftPlus:
                output = SoftPlus(value);
                break;
            case RNNFunction.ReLu:
                output = Relu(value);  //Check
                break;
            case RNNFunction.TanH:
                output = Tanh(value); 
                break;
            case RNNFunction.Sigmoid:
                output = Sigmoid(value);  //Check
                break;
            case RNNFunction.Input:
                output = input;  //Check
                break;
            default:
                output = value;
                break;
        }

        return output;
    }

    public float input;
    public void SetInput(float i)
    {
        input = i;
    }

    public float Sigmoid(float value)
    {
        return 1.0f / (1.0f + Mathf.Pow(2.71828f, -value));
    }
    public float Relu(float value)
    {
        return value < 0 ? 0 : value;
    }
    public float SoftPlus(float value)
    {
        return Mathf.Log(1 + Mathf.Pow(2.71828f, value));
    }
    public float Tanh(float value)
    {
        return (float)System.Math.Tanh(value);
    }

    public void RandomizeWeights(float range)
    {
        for(int c = 0; c < weights.Length; c++)
        {
            weights[c] = Random.Range(-range, range);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NN_Manager : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject carPrefab;
    [SerializeField] GameObject neuronPrefab;
    [SerializeField] int carCount = 16;
    [SerializeField] float evaluationTime = 10;
    [SerializeField] float mutability= 1.0f;

    void Start()
    {
            for(int c = 0; c < carCount; c++)
            {
                GameObject car = Instantiate(carPrefab, spawnPoint.position+Random.insideUnitSphere, spawnPoint.rotation, spawnPoint);
                if (car.transform.GetChild(0).childCount<=0)
                {
                    CreateNN(car.transform);
                }
            }
            Invoke("Examinate", evaluationTime);
    }

    void CreateNN(Transform car)
    {
        Transform iat=car.GetChild(0);

        int inputCount = 8;
        NN_Neuron[] inputLayer = new NN_Neuron[inputCount];
        for (int i = 0; i < inputCount; i++) {
            inputLayer[i] = Instantiate(neuronPrefab,iat).GetComponent<NN_Neuron>(); //<-Creamos 
            inputLayer[i].activationFunction = NN_Neuron.RNNFunction.Input;//Input para que reciba el valor que le damos de entrada desde el sensor
            inputLayer[i].name = "Input " +(i+1);
            inputLayer[i].RandomizeWeights(1f);
        }



        int middleCount = 21;
        NN_Neuron[] middleLayer = new NN_Neuron[middleCount];
        for (int i = 0;i<middleCount;i++)
        {
            middleLayer[i] = Instantiate(neuronPrefab, iat).GetComponent<NN_Neuron>(); //<-Creamos 
            middleLayer[i].activationFunction = NN_Neuron.RNNFunction.SoftPlus;//Input para que reciba el valor que le damos de entrada desde el sensor
            middleLayer[i].name = "Middle " + (i + 1);
            middleLayer[i].RandomizeWeights(1f);

        }
        //Connect to input
        FullConnect(middleLayer,inputLayer);
        RandomizeConnections(middleLayer);



        int outputCount = 3;
        NN_Neuron[] outputLayer= new NN_Neuron[outputCount];
        for (int i = 0; i < outputCount; i++)
        {
            outputLayer[i] = Instantiate(neuronPrefab, iat).GetComponent<NN_Neuron>();
            outputLayer[i].activationFunction = NN_Neuron.RNNFunction.TanH;
            outputLayer[i].name = "Output " + (i + 1);
        }
        FullConnect(outputLayer, middleLayer);
        RandomizeConnections(outputLayer);



        //Add SENSORS
        iat.gameObject.AddComponent<NN_Sensor>();
        NN_Sensor snsr=iat.gameObject.GetComponent<NN_Sensor>();
        snsr.outputs = outputLayer;
        snsr.inputs= inputLayer;
        snsr.carController = car.GetComponent<CarPhysics>();
    }


    public void FullConnect(NN_Neuron[] connect, NN_Neuron[] to)
    {
        for(int c = 0; c < connect.Length; c++)
        {
            connect[c].SetConnections(to,null);
        }
    }

    public void RandomizeConnections(NN_Neuron[] neurons)
    {
        foreach(NN_Neuron n in neurons)
        {
            n.RandomizeWeights(1f);
        }
    }


    public void Examinate()
    {
        NN_Sensor[] cars = FindObjectsOfType<NN_Sensor>();
        float min = float.MaxValue;
        float max = float.MinValue;
        
        foreach(NN_Sensor s in cars)
        {
            min = Mathf.Min(min, s.GetScore());
            max = Mathf.Max(max, s.GetScore());
        }
        float[] scores = new float[cars.Length];
        float average= (max + (max + min)/scores.Length)/2;
        Debug.Log("la media actual es:" + average);


        GameObject[] selected = Selection(cars, average);//Selected cars
        GameObject[] crossed = Cross(selected,carCount-selected.Length);
        
        Replacement(cars,selected,crossed);
        Mutation(crossed,mutability);

        Invoke("Examinate", evaluationTime);
    }


    public GameObject[] Selection(NN_Sensor[] ias, float grade)
    {
        //dar prioridad a los que tienen mejor nota y eliminar a los que tienen baja nota
        List<GameObject> l = new List<GameObject>();
        for (int i=0;i< ias.Length;i++)
        {
            if (ias[i].GetScore() >= grade)
            {
                l.Add(ias[i].transform.parent.gameObject);
            }
        }
        return l.ToArray();
    }
    

    public GameObject[] Cross(GameObject[] ias,int howMany)
    {
        List<GameObject> l = new List<GameObject>();

        GameObject prefab = ias[0];

        for (int c=0;c<howMany;c++)
        {
            GameObject newCar = Instantiate(prefab, spawnPoint.position + Random.insideUnitSphere, spawnPoint.rotation, spawnPoint);
            newCar.name = "NewGen";

            GameObject carA = ias[Random.Range(0, ias.Length)];
            GameObject carB = ias[Random.Range(0, ias.Length)];

            //Transform 
            int neuronCount =carA.transform.GetChild(0).childCount;
            for(int n = 0; n < neuronCount; n++)
            {
                NN_Neuron A = carA.transform.GetChild(0).GetChild(n).GetComponent<NN_Neuron>();
                NN_Neuron B = carB.transform.GetChild(0).GetChild(n).GetComponent<NN_Neuron>();
                NN_Neuron C = newCar.transform.GetChild(0).GetChild(n).GetComponent<NN_Neuron>();
                for(int w = 0; w < A.weights.Length; w++)
                {
                    C.weights[w]= Random.value>0.5f?A.weights[w]:B.weights[w];
                    
                }
            }
            l.Add(newCar);
        }
        
        return l.ToArray();
    }

    public void Mutation(GameObject[] ias,float m)
    {
        //pequeña alteración de los elementos.
        //por si mejoran o empeoran
        foreach(GameObject c in ias)
        {
            Transform ia = c.transform.GetChild(0);
            for(int i = 0; i < ia.childCount; i++)
            {
                NN_Neuron n = ia.GetChild(i).GetComponent<NN_Neuron>();
                for(int w = 0; w<n.weights.Length ;w++)
                {
                    n.weights[w]+=Random.Range(-m, m);
                }
            }
        }
    }

    public void Replacement(NN_Sensor[] olds,GameObject[] fathers,GameObject[] sons)
    {
        for (int c = 0; c < fathers.Length; c++)
        {
            GameObject car = Instantiate(fathers[c], spawnPoint.position + Random.insideUnitSphere, spawnPoint.rotation, spawnPoint);
            car.name ="OldGen";
        }

        for (int c = 0; c < olds.Length;c++)
        {
            Destroy(olds[c].transform.parent.gameObject);
        }
    }



    


}

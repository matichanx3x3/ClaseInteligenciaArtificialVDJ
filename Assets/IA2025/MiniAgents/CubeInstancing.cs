using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeInstancing : MonoBehaviour
{
    public Material[] materials= new Material[0];
    public int width  = 64;
    public int height = 64;
    int[,] table;
    public Mesh meshCube;
    // Start is called before the first frame update
    void Start()
    {
        Generate();
        Camera.main.transform.position = Vector3.right * ((width / 2)-0.5f) + (Vector3.up * ((height / 2)-0.5f)) - Vector3.forward*(width>height?width:height)*0.5f;
    }
    void BoxesLogic()
    {
        //table[y,x]
        //el y sale del for buscando recorrrer el height del canvas
        //el x sale del for buscando recorrer el widht del canvas
        for (int x = 1; x < height; x++)
        {
            for (int y = 1; y < width; y++)
            {
                switch (table[y,x])
                {
                    case 3:
                        if (table[y-1,x] == 0) //si es aire, pintar hacia abajo
                        {
                            table[y,x] = 0;
                            table[y-1,x] = 3;
                            break;
                        }
                        else
                        {
                            var random = Random.Range(0,2);
                            if (random == 0) // prefiere izquierda
                            {
                                if ((table[y-1,x] == 1 ||table[y-1,x] == 3) &&table[y-1,x+1] == 0) //la izquierda esta ocupada?
                                {
                                    table[y,x] = 0;
                                    table[y-1,x+1] = 3; //pos lo pone en la izquierda
                                    break;
                                }
                                else if ((table[y-1,x] == 1 ||table[y-1,x] == 3) &&table[y-1,x-1] == 0)//derecha ocupada?
                                {
                                    table[y,x] = 0;
                                    table[y-1,x-1] = 3; //pinta derecha
                                    break;  
                                }     
                                else
                                {
                                    table[y,x] = 3;
                                    break;
                                }
                                
                            }else //prefiere derecha
                            {
                                if ((table[y-1,x] == 1 ||table[y-1,x] == 3) &&table[y-1,x-1] == 0)//derecha ocupada?
                                {
                                    table[y,x] = 0;
                                    table[y-1,x-1] = 3; //pinta derecha
                                    break;  
                                }
                                else if ((table[y-1,x] == 1 ||table[y-1,x] == 3) &&table[y-1,x+1] == 0) //la izquierda esta ocupada?
                                {
                                    table[y,x] = 0;
                                    table[y-1,x+1] = 3; //pos lo pone en la izquierda
                                    break;
                                }
                                else
                                {
                                    table[y,x] = 3;
                                    break;
                                }
                            }
                        }
                    case 4:
                        //logica de agua
                    break;
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        Ray r =Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(r.origin, r.direction);
        Vector3 intersection;
        LinePlaneIntersection(out intersection, r.origin, r.direction, -Vector3.forward, Vector3.zero);
        int x = Mathf.RoundToInt(intersection.x);
        int y= Mathf.RoundToInt(intersection.y);
        try { 
        if (table[y, x] != 1)
        {
            if (Input.GetKey(KeyCode.Alpha1))
            {
                //aire, borra
                //Clean white
                table[y, x] = 0;
            }
            else if (Input.GetKey(KeyCode.Alpha2))
            {
                //bedrock, no se mueve
                //Clean white
                table[y, x] = 2;
            }
            else if (Input.GetKey(KeyCode.Alpha3))
            {
                //arena, se cae de donde spawnea
                //Clean white
                table[y, x] = 3;
            }
            else if (Input.GetKey(KeyCode.Alpha4))
            {
                // agua, cae y se comporta como agua. tiene que atravesar la arena y posicionarse por debajo.
                //Clean white
                table[y, x] = 4;
            }
            else if (Input.GetKey(KeyCode.Alpha5))
            {
                //Clean white
                //table[y, x] = 5;
            }
        }
        }catch
        {

        }
        DrawCubes();
    }

    private void FixedUpdate()
    {
        BoxesLogic();
    }



    [ContextMenu("Generate")]
    public void Generate()
    {
        width = width >= 256 ? 256 : width;
        width = width <= 8 ? 8: width;
        height = height >= 256 ? 256 : height;
        height = height <= 8 ? 8 : height;

        table = new int[height, width];

        for(int c = 0; c < width;c++)
        {
            table[0, c] = 1;
            table[height-1, c] = 1;
        }
        for (int c = 0; c < height; c++)
        {
            table[c, 0] = 1;
            table[c, width-1] = 1;
        }

    }

    public void DrawCubes()
    {
        if (table != null)
        {
            List<Matrix4x4>[] matrixBatches = new List<Matrix4x4>[materials.Length];
            for (int c = 0; c < matrixBatches.Length; c++)
            {
                matrixBatches[c] = new List<Matrix4x4>();

            }

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matrixBatches[table[y, x]].Add(Matrix4x4.TRS(Vector3.up * y + Vector3.right * x, Quaternion.identity, Vector3.one));
                }
            }

            for (int c = 0; c < matrixBatches.Length; c++)
            {
                int start = 0;
                int end = matrixBatches[c].Count;
                while (start!=end)
                {
                    int currentLength = end - start > 1023?1023:end-start;
                    List<Matrix4x4> miniBatch = matrixBatches[c].GetRange(start, currentLength);
                    Graphics.DrawMeshInstanced(meshCube, 0, materials[c], miniBatch);
                    start += currentLength;
                }
                
            }
        }

    }

    public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
    {

        float length;
        float dotNumerator;
        float dotDenominator;
        Vector3 vector;
        intersection = Vector3.zero;

        //calculate the distance between the linePoint and the line-plane intersection point
        dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
        dotDenominator = Vector3.Dot(lineVec, planeNormal);

        //line and plane are not parallel
        if (dotDenominator != 0.0f)
        {
            length = dotNumerator / dotDenominator;

            //create a vector from the linePoint to the intersection point
            vector = lineVec.normalized * length;
            //vector = SetVectorLength(lineVec, length);

            //get the coordinates of the line-plane intersection point
            intersection = linePoint + vector;

            return true;
        }

        //output not valid
        else
        {
            return false;
        }
    }
}

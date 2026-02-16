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

        //nueva aproximación -> buscar que haga comportamientos a la derecha si el frame es par o izquierda si es impar
        bool par= Time.frameCount%2 == 0; //si es divisible entre 2 es par -> mover derecha
        // si es impar -> izquierda
        //concepto de bucle dinamico, que busca cambiar el comportamiento segun el momento, en este caso si es par o impar.
        int startX = par ? 1 : width - 2;      // empieza en 1 o en el penúltimo
        int endX   = par ? width - 1 : 0;      // Terminamos en el borde opuesto
        int stepX  = par ? 1 : -1;             // suma (+1) o resta (-1)
        

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = startX; x != endX; x += stepX)
            {
                if (x <= 0 || x >= width - 1 || y <= 0 || y >= height - 1) continue;
                switch (table[y,x])
                {
                    case 3:
                        if (table[y-1,x] == 0) //si es aire, pintar hacia abajo
                        {
                            table[y,x] = 0;
                            table[y-1,x] = 3;
                            break;
                        }
                        else if (table[y-1,x] == 4)
                        {
                            table[y,x] = 0;
                            table[y-1,x] = 3;
                            table[y,x] = 4;
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
                        if (table[y-1,x] == 0) 
                        {
                            //caer
                            table[y,x] = 0;
                            table[y-1,x] = 4;
                            break;
                        }
                        else if (table[y-1, x-1] == 0)
                        {   //diagonal izq
                            table[y,x] = 0;
                            table[y-1,x-1] = 4;
                            break;
                        }
                        else if (table[y-1, x+1] == 0)
                        {
                            //diag derech
                            table[y,x] = 0;
                            table[y-1,x+1] = 4;
                            break;
                        }
                        else
                        {
                            if (par)
                            {
                                //hay espacio a la derecha? -> ve a la izquierda
                                if (table[y, x-1] == 0)
                                {
                                    table[y, x] = 0;
                                    table[y, x-1] = 4;
                                    break;
                                } 
                            }
                            else
                            {
                                //hay espacio a la izquierda? -> ve a la derecha
                                if (table[y, x+1] == 0)
                                {
                                    table[y, x] = 0;
                                    table[y, x+1] = 4;
                                    break;
                                }
                            }
                        }
                    break;
                    case 5:

                        if (table[y-1,x] == 0) 
                        {
                            //caer
                            table[y,x] = 0;
                            table[y-1,x] = 5;
                            break;
                        } 
                        /*else if ( table[y-1,x] != 0 && table[y-1,x] == 4)
                        {
                            Debug.Log("entra en agua");  
                            table[y,x] = 4;
                            table[y-1,x] = 5;
                            contador ++;
                            if(contador > 3)
                            {
                                table[y-1,x] = 4;
                                table[y+1,x] = 5;
                                if(contador > 6)
                                {
                                    table[y,x] = 0;
                                    table[y-1,x] = 5;  
                                    contador = 0;
                                }
                                break;
                            }
                            break;
                        } */
                        else if (table[y-1,x] != 0 && table[y-1,x] == 5)
                        {
                            Debug.Log("reemplaza");    
                            table[y,x] = 5;
                            table[y-1,x]=0;
                            break;
                        }
                    break;
                }
            }
        }

        // lo de abajo funciona, pero no es una aproximación "pensada"
        /*for (int x = 1; x < height; x++)
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
                        else if (table[y-1,x] == 4)
                        {
                            table[y,x] = 0;
                            table[y-1,x] = 3;
                            table[y,x] = 4;
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
                        if (table[y-1,x] == 0) //si es aire, pintar hacia abajo
                        {
                            table[y,x] = 0;
                            table[y-1,x] = 4;
                            break;
                        }
                        else if (table[y-1, x-1] == 0)
                        {
                            table[y,x] = 0;
                            table[y-1,x-1] = 4;
                            break;
                        }
                        else if (table[y-1, x+1] == 0)
                        {
                            table[y,x] = 0;
                            table[y-1,x+1] = 4;
                            break;
                        }
                        else if(table[y,x+1] == 0 || table[y,x-1] == 0)
                        {
                            var random = Random.Range(0,2);
                            switch (random)
                            {
                                case 0:
                                    if (table[y,x+1] == 0) //la derecha esta ocupada?
                                    {
                                        table[y,x] = 0;
                                        table[y,x+1] = 4; //pos lo pone en la derecha
                                        break;  
                                    }
                                    else if (table[y,x-1] == 0)//izquierda ocupada?
                                    {
                                        table[y,x] = 0;
                                        table[y,x-1] = 4; //pinta izquierda
                                        break;  
                                    }                              
                                break;
                                case 1:
                                    if (table[y,x-1] == 0)//izquierda ocupada?
                                    {
                                        table[y,x] = 0;
                                        table[y,x-1] = 4; //pinta izquierda
                                        break;  
                                    }
                                    else if (table[y,x+1] == 0) //la derecha esta ocupada?
                                    {
                                        table[y,x] = 0;
                                        table[y,x+1] = 4; //pos lo pone en la derecha
                                        break;
                                    }
                                break;
                            }
                        }
                        else
                        {
                            table[y,x] = 4;
                            break;
                        }
                    break;
                }
            }
        }*/
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
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                //mini roca
                table[y, x] = 5;
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

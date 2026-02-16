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
        //For each cell we are going to apply a set of rules.
        for (int y = 1; y < height-1; y++)
        {
            for (int x = 1; x < width-1; x++)
            {
                //0 Empty
                //1 Border(can not be removed)
                //2 Wall can be removed
                //3 Sand it has a set of rules
                //4 Water it has a set of rules

                if (table[y, x] == 3)//If the cell is sand...
                {
                    //If there is an empty hole at the bottom just move this square 1 position down
                    if (table[y-1,x] == 0)
                    {
                        table[y, x] = 0;
                        table[y - 1, x] = 3;
                    }
                    else if (table[y - 1, x +1]== 0 && table[y,x+1] == 0)
                    {
                        table[y, x] = 0;
                        table[y - 1, x + 1] = 3;
                    }
                    else if (table[y - 1, x - 1] == 0 && table[y, x - 1] == 0)
                    {
                        table[y, x] = 0;
                        table[y - 1, x - 1] = 3;
                    }

                }

                //If there is not and empty hole at the bottom 
                //Take a look af the boto right. If the bottom right is empty
                //then move this square to that position.

                //Same for the left


                //If there is an empty hole at the bottom them like the sand the water
                //goes 1 square down.

                //if there is an empty hole at the right or left choose any
                //side randomly and move the square there
                

            }
        }


    }
    private void FixedUpdate()
    {
        BoxesLogic();
    }
    // Update is called once per frame
    void Update()
    {
        Ray r =Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(r.origin, r.direction);
        Vector3 intersection;
        LinePlaneIntersection(out intersection, r.origin, r.direction, -Vector3.forward, Vector3.zero);
        int x = Mathf.RoundToInt(intersection.x);
        int y= Mathf.RoundToInt(intersection.y);
        try { 
        if (table[y, x] != 1)
        {
            if (Input.GetKey(KeyCode.Alpha1))
            {
                //Clean white
                table[y, x] = 0;
            }
            else if (Input.GetKey(KeyCode.Alpha2))
            {
                //Clean white
                table[y, x] = 2;
            }
            else if (Input.GetKey(KeyCode.Alpha3))
            {
                //Clean white
                table[y, x] = 3;
            }
            else if (Input.GetKey(KeyCode.Alpha4))
            {
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
        BoxesLogic();
        DrawCubes();
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HormiguitasOrdenando_Plantilla : MonoBehaviour
{
    public class Ant
    {
        public int x;
        public int y;
        int xRange;
        int yRange;
        byte[,] map;
        byte charge;
        public Ant(int rx, int ry,byte[,] theMap)
        {
            x = Random.Range(0, rx);
            y = Random.Range(0, ry);
            xRange =rx;
            yRange = ry;
            map = theMap;
            charge = 0;
        }
        //CON 7 Y 4 FUNCIONA
        public void StepAnt()
        {
            if(charge == 0)
            {
                charge = map[y,x];
                map[y,x] = 0;
            }
            else
            {
                if (CountElements )
                {
                    
                }
                map[y,x] = charge;
            }
            
            int rdm= Random.Range(0,4);

            switch (rdm)
            {
                case 0:
                    if(x < xRange-1)
                        x += 1;
                    else
                        x = xRange-2; 
                break;
                case 1:
                    if(y < yRange-1)
                        y+=1;
                    else
                        y = yRange-2; 
                break;
                case 2:
                    if(x > 0)
                        x -= 1;
                    else
                        x = 1; 
                break;
                case 3:
                    if(y > 0)
                        y -= 1;
                    else
                        y = 1;
                break;
            } 
            
           //recoge con un radio de 7 y deja con un radio de 4.
            //si no tiene nada tiene que coger algo.
            /**/

            //cuando tiene que coger algo?
            //si hay muy pocas cosas de este tipo a su alrededor.
            //es por probabilidad. -> si a mi alrededor hay 25 cuadrado y hay una sola azul, se la lleva.
            //entonces cuando tiene una caja azul y de forma aleatoria llegara a un 
            
        }


        int CountElements(int type, int px,int py,int ks) //px y px -> punto donde esta la hormiga. ks (rango del radio que la hormiga puede ver).
        {
            int equal = 0;
            for (int j = py; j < ks; j++)
            {
                for (int i = px; i < ks; i++)
                {
                    
                }
            }
            // contar elementos cerca
            //bucle de las posiciones de la hormiga la pos y -ks hasta la pos y - ks.
            //cuantas casillas son del tipo -> =++; (usar bucles anidados)
            return equal;
        }
    }

    // Start is called before the first frame update
    public byte[,] grid;
    public int heigh=40;
    public int width=40;
    public int antsNumber=64;
    public bool simulate = true;

    public Ant[] antsList;
    void Start()
    {
        grid = new byte[heigh, width];

        for (int y = 0; y < heigh; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[y, x] = Random.Range(0.0f, 1.0f) > 0.38f ? (byte)0 : (byte)Random.Range(1,5);
            }
        }
        antsList = new Ant[antsNumber];
        for(int a = 0; a < antsNumber; a++)
        {
            antsList[a] = new Ant(width,heigh,grid);
        }
    }


    // Update is called once per frame
    
    void Update()
    {
        for (int a = 0; a < antsNumber; a++)
        {
            antsList[a].StepAnt();
        }
    }


    private void OnDrawGizmos()
    {
        if (grid != null) { 
        Color actualColor = Color.white;
        Color nextColor = Color.white;
        Color clean = Color.white;
        Color cookie= new Color(0.6f,0.4f,0.4f);
        Color water = new Color(0.3f, 0.5f, 0.9f);
        Color meat = new Color(0.9f, 0.2f, 0.2f);
        Color leaf = new Color(0.3f, 0.8f, 0.3f);
        for (int y=0;y<heigh;y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[y, x]==0)
                {
                    nextColor = clean;
                }else if (grid[y, x] == 1)
                {
                    nextColor = cookie;
                }
                else if (grid[y, x] == 2)
                {
                    nextColor = water;
                }
                else if (grid[y, x] == 3)
                {
                    nextColor = meat;
                }
                else if (grid[y, x] == 4)
                {
                    nextColor = leaf;
                }
                if (nextColor != actualColor)
                {
                    actualColor = nextColor;
                    Gizmos.color = actualColor;
                    
                }
                Gizmos.DrawCube(new Vector3(x,y,0),Vector3.one);
            }
        }

            Gizmos.color = new Color(0.0f, 0.0f, 0.0f, 0.66f);
            for (int a = 0; a < antsNumber; a++)
            {
                Gizmos.DrawCube(new Vector3(antsList[a].x, antsList[a].y,-1), Vector3.one); 
            }

        }
    }
}

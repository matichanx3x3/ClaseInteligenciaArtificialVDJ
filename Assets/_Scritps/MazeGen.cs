using System;
using System.Collections;
using UnityEngine;

public enum TypeOfBlock
{
    Background = 0,
    MazeWall = 1,
    Path = 2,
    Entry = 3,
    Exit = 4
}

public class MazeGen : MonoBehaviour
{
    [Header("Visual")]
    public Mesh meshCube;
    public Material[] materials = new Material[0];

    [Header("Grid")]
    public int X = 64;
    public int Y = 64;
    public float cellSize = 1f;

    [Header("Generation")]
    public float stepDelay = 0.01f;

    public TypeOfBlock[,] matrizObjetos;
    private MeshRenderer[,] cellRenderer;

    [SerializeField] int centerX;
    [SerializeField] int centerY;

    void Start()
    {
        Generate();
    }
    public void Generate()
    {
        X = Mathf.Clamp(X, 8, 256);
        Y = Mathf.Clamp(Y, 8, 256);

        centerX = X / 2;
        centerY = Y / 2;

        matrizObjetos = new TypeOfBlock[X, Y];
        cellRenderer = new MeshRenderer[X, Y];
        StopAllCoroutines();
        StartCoroutine(GenerateAll());
    }

    private IEnumerator GenerateAll()
    {
        yield return StartCoroutine(GenerateBackground());
        
        yield return StartCoroutine(GenerateBorder());
        
        yield return StartCoroutine(GenerateBinaryTree());
        
        Debug.Log("¡Generación completada!");
    }

    private IEnumerator GenerateBackground()
    {
        //generacion en espiral
        //ref: https://stackoverflow.com/questions/398299/looping-in-a-spiral
        int totalCubes = X * Y;
        int totalCreated = 0;

        if(TryCreateBackground(centerX,centerY)) totalCreated++;

        int stepLen = 1;
        int x = centerX;
        int y = centerY;
        while (totalCreated < totalCubes)
        {
            // derecha
            for (int i = 0; i < stepLen && totalCreated < totalCubes; i++)
            {
                x++;
                if (TryCreateBackground(x, y)) totalCreated++;
                yield return StepWait();
            }

            // arriba
            for (int i = 0; i < stepLen && totalCreated < totalCubes; i++)
            {
                y++;
                if (TryCreateBackground(x, y)) totalCreated++;
                yield return StepWait();
            }

            stepLen++;

            // izquierda
            for (int i = 0; i < stepLen && totalCreated < totalCubes; i++)
            {
                x--;
                if (TryCreateBackground(x, y)) totalCreated++;
                yield return StepWait();
            }

            // abajo
            for (int i = 0; i < stepLen && totalCreated < totalCubes; i++)
            {
                y--;
                if (TryCreateBackground(x, y)) totalCreated++;
                yield return StepWait();
            }

            stepLen++;
        }
    }
    private IEnumerator GenerateBorder()
    {
        // PUNTO 0,0
        for (int i = 0; i < Y; i++)
        {
            SetCell(0, i, TypeOfBlock.MazeWall);
            yield return StepWait();
        }

        // PUNTO X,Y
        for (int i = Y-1; i > 0; i--)
        {
            SetCell(X-1, i, TypeOfBlock.MazeWall);
            yield return StepWait();
        }

        // PUNTO 0,Y
        for (int i = 1; i < X; i++)
        {
            SetCell(i, Y-1, TypeOfBlock.MazeWall);
            yield return StepWait();
        }

        // PUNTO X,0
        for (int i = X-1; i > 0; i--)
        {
            SetCell(i, 0,TypeOfBlock.MazeWall);
            yield return StepWait();
        }
    }

    private IEnumerator GenerateBinaryTree()
    {
        for (int x = 1; x < X - 1; x++)
        {
            for (int y = 1; y < Y - 1; y++)
            {
                SetCell(x, y, TypeOfBlock.MazeWall);
                yield return StepWait();
            }
        }
        yield return StepWait();
        for (int y = 1; y < Y - 1; y += 2)
        {
            for (int x = 1; x < X - 1; x += 2)
            {
                // 1. Convertimos la "habitación" actual en camino
                SetCell(x, y, TypeOfBlock.Path);
                yield return StepWait();

                // 2. Evaluamos si PODEMOS ir hacia arriba o hacia la derecha
                bool canGoUp = (y + 1 < Y - 1); // ¿Hay espacio arriba sin tocar el borde?
                bool canGoRight = (x + 1 < X - 1); // ¿Hay espacio a la derecha sin tocar el borde?

                // 3. Lógica de decisión
                if (canGoUp && canGoRight)
                {
                    // ¡Aquí viene la magia! 
                    // Lanza una moneda al aire (usa Random.Range)
                    // Si sale 0, rompe la pared de Arriba (SetCell en x, y+1)
                    // Si sale 1, rompe la pared de la Derecha (SetCell en x+1, y)
                    
                    // [ESCRIBE TU LÓGICA AQUÍ]
                    int rnd = UnityEngine.Random.Range(0,2);
                    switch (rnd)
                    {
                        case 0:
                            SetCell(x, y+1, TypeOfBlock.Path);
                            yield return StepWait();
                        break;
                        case 1:
                            SetCell(x+1, y, TypeOfBlock.Path);
                            yield return StepWait();
                        break;
                    }
                }
                else if (canGoUp)
                {
                    // Solo podemos ir arriba, así que obligatoriamente rompemos la pared de arriba
                    SetCell(x, y + 1, TypeOfBlock.Path);
                    yield return StepWait();
                }
                else if (canGoRight)
                {
                    // Solo podemos ir a la derecha, obligatoriamente rompemos la pared de la derecha
                    SetCell(x+1, y, TypeOfBlock.Path);
                    yield return StepWait();
                }
                
                yield return StepWait();
            }
        }

    }

    YieldInstruction StepWait()
    {
        if (stepDelay <= 0f) return null; // un bloque por frame
        return new WaitForSeconds(stepDelay);
    }

    private bool TryCreateBackground(int gx, int gy)
    {
        if(gx < 0 || gx >= X || gy < 0 || gy >= Y)
            return false;
        
        CreateCube(gx, gy);
        return true;
    }

    void SetCell(int gx, int gy, TypeOfBlock type)
    {
        if (gx < 0 || gx >= X || gy < 0 || gy >= Y) return;

        matrizObjetos[gx, gy] = type;

        var mr = cellRenderer[gx, gy];
        if (mr != null)
            mr.sharedMaterial = materials[(int)type];
    }

    public void CreateCube(int gx, int gy)
    {
        GameObject go = new GameObject($"Cell_{gx}_{gy}");

        go.transform.SetParent(transform, worldPositionStays: false);

        float px = (gx - centerX) * cellSize;
        float pz = (gy - centerY) * cellSize;

        go.transform.localPosition = new Vector3(px, 0f, pz);

        //sharedmesh y sharedmaterial conviene cuando se instancian muchos objetos identicos,
        //ayuda para que no se dupliquen estos elementos de forma innecesaria
        //asi libra memoria de instancias innecesarias.
        
        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = meshCube;

        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = materials[(int)TypeOfBlock.Background];
        cellRenderer[gx, gy] = mr;
        //Debug.Log("el cubo se instancio en width: "+gx +" y height: "+ gy + " con el valor: "+ TypeOfBlock.Background);

    }
    
}

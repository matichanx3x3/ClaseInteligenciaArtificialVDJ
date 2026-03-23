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

    void Update()
    {
        if (GetComponent<MazeSolver>().isFinished == true)
        { //solo cuando se haya terminado una vez el laberinto.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GetComponent<MazeSolver>().isFinished = false;
                StopAllCoroutines();
                StartCoroutine(GenerateBorder());
                StartCoroutine(GenerateBinaryTree());
            }
        }
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
        //referencia: https://weblog.jamisbuck.org/2011/2/1/maze-generation-binary-tree-algorithm

        // 1: llenar el espacio de paredes
        for (int x = 1; x < X - 1; x++)
        {
            for (int y = 1; y < Y - 1; y++)
            {
                SetCell(x, y, TypeOfBlock.MazeWall);
            }
        }
        yield return StepWait();

        // 2: genera puntos aislados, esto pq si fuera punto por punto rellenaria TOOODO el espacio y no se busca eso
        // se recorre desde abajo hasta arriba de izq a der.
        for (int y = 1; y < Y - 1; y += 2)
        {
            for (int x = 1; x < X - 1; x += 2)
            {
                SetCell(x, y, TypeOfBlock.Path);
                yield return StepWait(); // convierte a camino paso por paso
            }
        }

        yield return new WaitForSeconds(0.5f); 

        // 3: se aplica el algoritmo de binary tree
        for (int y = 1; y < Y - 1; y += 2)
        {
            for (int x = 1; x < X - 1; x += 2)
            {
                
                bool hasSouth = (y > 1);
                bool hasWest = (x > 1);

                // se empieza a aplicar las reglas del algoritmo
                if (hasSouth && hasWest)
                {
                    // si tiene vecino en ambos lados, elige aleatoriamente
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        SetCell(x, y - 1, TypeOfBlock.Path); // abre para el sur
                    }
                    else
                    {
                        SetCell(x - 1, y, TypeOfBlock.Path); // abre para el oeste
                    }
                    yield return StepWait();
                }
                else if (hasSouth && !hasWest)
                {
                    // tiene vecino en el sur?
                    SetCell(x, y - 1, TypeOfBlock.Path);
                    yield return StepWait(); 
                }
                else if (!hasSouth && hasWest)
                {
                    // tiene vecino en el oeste?
                    SetCell(x - 1, y, TypeOfBlock.Path);
                    yield return StepWait(); 
                }
                //si no tiene ningun vecino (es decir que esta en la esquina inicial, no hace nada y pasaria a la siguiente habitacion)
            }
        }

        // entrada y salida
        yield return GenerateEntryAndExit();
    }

private IEnumerator GenerateEntryAndExit()
    {
        // se necesita asegurarse que tenga que conectar con un pasillo. (al parece siendo impar siempre cumple.)
        int randomEntryPos;
        do {
            randomEntryPos = UnityEngine.Random.Range(1, X - 1);
        } while (randomEntryPos % 2 == 0);

        // la entrada es aleatoria del borde inferior o izquierdo
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            SetCell(randomEntryPos, 0, TypeOfBlock.Entry); // Borde inferior
        }
        else
        {
            SetCell(0, randomEntryPos, TypeOfBlock.Entry); // Borde izquierdo
        }
        yield return StepWait();

        // la entrada es aleatoria del borde superior o derecha
        int randomExitPos;
        do {
            randomExitPos = UnityEngine.Random.Range(1, X - 1);
        } while (randomExitPos % 2 == 0); 

        //cálculo matemático para el borde máximo válido.
        // matriz es 40, el último pasillo estará entre 37 o 39
        int maxValidEdge = (X % 2 == 0) ? X - 3 : X - 2;

        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            // Borde superior
            SetCell(randomExitPos, maxValidEdge + 1, TypeOfBlock.Exit);
            SetCell(randomExitPos, maxValidEdge, TypeOfBlock.Path); // busca dar conexión
        }
        else
        {
            // Borde derecho
            SetCell(maxValidEdge + 1, randomExitPos, TypeOfBlock.Exit);
            SetCell(maxValidEdge, randomExitPos, TypeOfBlock.Path); // busca dar conexión
        }
        yield return StepWait();
        GetComponent<MazeSolver>().StartSolving(); //empieza a resolver el laberinto
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

    }
    public MeshRenderer GetCellRenderer(int x, int y)
    {
        return cellRenderer[x, y];
    }
    
}

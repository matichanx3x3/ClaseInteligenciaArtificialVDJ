using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Node
{
    public int x, y;
    public Node parent; // de que celda venimos
    
    // Para A*
    public int gCost; // coste inicial
    public int hCost; // coste para el final (heuristico)
    public int fCost => gCost + hCost; // coste total

    public Node(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}
public class MazeSolver : MonoBehaviour
{
    [Header("Ajustes del Pathfinding")]
    [Tooltip("Activar para usar A* (con heurística). Desactivar para usar BFS (sin heurística).")]
    public bool useHeuristic = false;
    public float searchStepDelay = 0.05f;

    [Header("Materiales Visuales")]
    public Material exploredMaterial; // Color de las celdas que el algoritmo ha revisado (ej. Amarillo)
    public Material finalPathMaterial; // Color de la solución final (ej. Azul o Rojo)

    // Referencias a los datos de MazeGen
    private MazeGen mazeGenerator;
    private int startX, startY;
    private int exitX, exitY;

    void Awake()
    {
        mazeGenerator = GetComponent<MazeGen>();
    }

    // Este método se llamará después de que termine la Parte 1
    public void StartSolving()
    {
        FindStartAndExitPoints();
        StartCoroutine(SolveMazeCoroutine());
    }

    // 2. Buscamos dónde están las casillas Entry y Exit generadas aleatoriamente
    private void FindStartAndExitPoints()
    {
        for (int x = 0; x < mazeGenerator.X; x++)
        {
            for (int y = 0; y < mazeGenerator.Y; y++)
            {
                if (mazeGenerator.matrizObjetos[x, y] == TypeOfBlock.Entry)
                {
                    startX = x;
                    startY = y;
                }
                else if (mazeGenerator.matrizObjetos[x, y] == TypeOfBlock.Exit)
                {
                    exitX = x;
                    exitY = y;
                }
            }
        }
    }

    // 3. El Bucle Principal del Pathfinding
    private IEnumerator SolveMazeCoroutine()
    {
        List<Node> openList = new List<Node>();
        HashSet<string> closedSet = new HashSet<string>(); // Usamos un string "x,y" para búsquedas rápidas

        Node startNode = new Node(startX, startY);
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode;

            // ---------------------------------------------------------
            // LA DIFERENCIA ENTRE LOS DOS ALGORITMOS (El Flag)
            // ---------------------------------------------------------
            if (useHeuristic)
            {
                // A* (Con Heurística): Sacar el nodo con el menor coste fCost
                currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].fCost < currentNode.fCost || 
                       (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                    {
                        currentNode = openList[i];
                    }
                }
            }
            else
            {
                //referencia: https://en.wikipedia.org/wiki/Breadth-first_search
                // Saca el primer nodo que entró
                currentNode = openList[0];
            }

            openList.Remove(currentNode);
            closedSet.Add($"{currentNode.x},{currentNode.y}");

            // Pinto la celda para que se vea cómo busca, EXCEPTO si es la entrada o salida
            if (!IsStartOrExit(currentNode.x, currentNode.y))
            {
                mazeGenerator.GetCellRenderer(currentNode.x, currentNode.y).sharedMaterial = exploredMaterial;
            }
            
            yield return new WaitForSeconds(searchStepDelay); // Punto extra: Generación paso a paso

            // ---------------------------------------------------------
            // ¿HEMOS LLEGADO A LA SALIDA?
            // ---------------------------------------------------------
            if (currentNode.x == exitX && currentNode.y == exitY)
            {
                yield return TracePathBack(currentNode); // Dibuja la línea final
                yield break; // Termina la corrutina
            }

            // ---------------------------------------------------------
            // EVALUAR VECINOS (Arriba, Abajo, Izquierda, Derecha)
            // ---------------------------------------------------------
            List<Node> neighbors = GetValidNeighbors(currentNode);

            foreach (Node neighbor in neighbors)
            {
                if (closedSet.Contains($"{neighbor.x},{neighbor.y}"))
                    continue; // Ya evaluamos esta celda, la ignoramos

                // Coste desde el inicio hasta el vecino (Avanzar 1 casilla cuesta 10 puntos)
                int newMovementCostToNeighbor = currentNode.gCost + 10; 

                // Si el vecino es nuevo, o si hemos encontrado un camino más corto hacia él
                Node nodeInOpenList = openList.Find(n => n.x == neighbor.x && n.y == neighbor.y);
                
                if (nodeInOpenList == null || newMovementCostToNeighbor < nodeInOpenList.gCost)
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = CalculateManhattanDistance(neighbor.x, neighbor.y, exitX, exitY);
                    neighbor.parent = currentNode;

                    if (nodeInOpenList == null)
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning("No se encontró ningún camino. (Esto no debería pasar en un Binary Tree)");
    }

    // 4. Retraza el camino desde la salida hasta la entrada usando los nodos "padre"
    private IEnumerator TracePathBack(Node endNode)
    {
        List<Node> finalPath = new List<Node>();
        Node currentNode = endNode;

        // 1. Recopilamos todos los nodos desde la Salida hasta la Entrada
        while (currentNode != null)
        {
            finalPath.Add(currentNode);
            currentNode = currentNode.parent;
        }

        // 2. Le damos la vuelta a la lista para que empiece en la Entrada y termine en la Salida
        finalPath.Reverse();

        // 3. Pintamos el camino paso a paso
        foreach (Node pathNode in finalPath)
        {
            // Opcional: Si no quieres repintar la casilla verde de entrada o roja de salida,
            // puedes dejar este IF. Si prefieres que la línea los cubra, quítalo.
            if (!IsStartOrExit(pathNode.x, pathNode.y))
            {
                mazeGenerator.GetCellRenderer(pathNode.x, pathNode.y).sharedMaterial = finalPathMaterial;
                yield return new WaitForSeconds(searchStepDelay * 2f); // Un poco más lento para darle drama visual
            }
        }

        Debug.Log("¡Laberinto resuelto desde el inicio hasta el final!");
    }

    // --- FUNCIONES AUXILIARES ---

    private List<Node> GetValidNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        // Solo evalúa Cruz (Arriba, Abajo, Izquierda, Derecha), no diagonales.
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = node.x + dx[i];
            int checkY = node.y + dy[i];

            // Comprueba límites de la matriz y si el bloque es caminable
            if (checkX >= 0 && checkX < mazeGenerator.X && checkY >= 0 && checkY < mazeGenerator.Y)
            {
                TypeOfBlock type = mazeGenerator.matrizObjetos[checkX, checkY];
                if (type == TypeOfBlock.Path || type == TypeOfBlock.Entry || type == TypeOfBlock.Exit)
                {
                    neighbors.Add(new Node(checkX, checkY));
                }
            }
        }
        return neighbors;
    }

    private int CalculateManhattanDistance(int x1, int y1, int x2, int y2)
    {
        return (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2)) * 10;
    }

    private bool IsStartOrExit(int x, int y)
    {
        return (x == startX && y == startY) || (x == exitX && y == exitY);
    }
}

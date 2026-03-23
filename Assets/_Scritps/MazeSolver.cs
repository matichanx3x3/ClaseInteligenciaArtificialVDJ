using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Node
{
    public int x, y;
    public Node parent; // de que celda venimos
    
    // para A*
    public int gCost; // coste inicial
    public int hCost; // coste para el final
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
    public Material exploredMaterial;
    public Material finalPathMaterial;

    // refs mazegen
    private MazeGen mazeGenerator;
    private int startX, startY;
    private int exitX, exitY;

    public bool isFinished = false;

    void Awake()
    {
        mazeGenerator = GetComponent<MazeGen>();
    }

    // lo empieza cuando se llame (cuando termina de resolver la gen)
    public void StartSolving()
    {
        FindStartAndExitPoints();
        StartCoroutine(SolveMazeCoroutine());
    }

    // se busca dónde están las casillas entry y exit generadas aleatoriamente
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

    // bucle inicial de pathfinding.
    private IEnumerator SolveMazeCoroutine()
    {
        List<Node> openList = new List<Node>();
        HashSet<string> closedSet = new HashSet<string>(); // Usamos un string "x,y" para búsquedas rápidas

        Node startNode = new Node(startX, startY);
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode;

            // ambas propuestas de algoritmos
            if (useHeuristic)
            {
                // A* con heuristica: sacar el nodo con el menor coste
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
                // Sin heuristica: Saca el primer nodo que entró
                currentNode = openList[0];
            }

            openList.Remove(currentNode);
            closedSet.Add($"{currentNode.x},{currentNode.y}");

            // pinta las celdas
            // EXCEPTO si es la entrada o salida
            if (!IsStartOrExit(currentNode.x, currentNode.y))
            {
                mazeGenerator.GetCellRenderer(currentNode.x, currentNode.y).sharedMaterial = exploredMaterial;
            }
            
            yield return new WaitForSeconds(searchStepDelay);

            // ya se esta en la salida?
            if (currentNode.x == exitX && currentNode.y == exitY)
            {
                yield return TracePathBack(currentNode); // Dibuja la línea final
                yield break;
            }

            // evalua vecinos
            List<Node> neighbors = GetValidNeighbors(currentNode);

            foreach (Node neighbor in neighbors)
            {
                if (closedSet.Contains($"{neighbor.x},{neighbor.y}"))
                    continue; // si ya esta evaluada la celda, solo continua

                // avanza al vecino segun un coste.
                int newMovementCostToNeighbor = currentNode.gCost + 10; 

                // si el vecino es nuevo, se busca el camino más corto.
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

    // metodo para trazar el camino
    private IEnumerator TracePathBack(Node endNode)
    {
        List<Node> finalPath = new List<Node>();
        Node currentNode = endNode;

        // se recolecta los nodos desde el final al inicio.
        while (currentNode != null)
        {
            finalPath.Add(currentNode);
            currentNode = currentNode.parent;
        }

        // unicamente para que empiece desde el inicio al final.
        finalPath.Reverse();

        // se pinta todo el camino
        foreach (Node pathNode in finalPath)
        {
            if (!IsStartOrExit(pathNode.x, pathNode.y))
            {
                mazeGenerator.GetCellRenderer(pathNode.x, pathNode.y).sharedMaterial = finalPathMaterial;
                yield return new WaitForSeconds(searchStepDelay * 2f);
            }
        }

        isFinished = true;
    }


    private List<Node> GetValidNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        // Solo evalúa arriba, abajo, izquierda, derecha
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarPathmaker : MonoBehaviour
{
    private TileType[,] grid;
    private int gridSizeX, gridSizeY;

    public AStarPathmaker(TileType[,] grid)
    {
        this.grid = grid;
        gridSizeX = grid.GetLength(1);
        gridSizeY = grid.GetLength(0);
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        // Initialize data structures.
        PriorityQueue<Vector2Int> openSet = new PriorityQueue<Vector2Int>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, target);

        while (!openSet.IsEmpty)
        {
            Vector2Int current = openSet.Dequeue();

            if (current == target)
                return ReconstructPath(cameFrom, current);

            closedSet.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float tentativeGScore = gScore[current] + Distance(current, neighbor);

                if (!openSet.Contains(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, target);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }
        // No path found.
        return new List<Vector2Int>();
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        // Calculate the heuristic (estimated) distance between two points (e.g., Manhattan distance).
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    private float Distance(Vector2Int a, Vector2Int b)
    {
        float distance = 0.0f;

        if (grid[b.y, b.x] == TileType.OuterWall)
        {
            distance = 2.0f; // Set distance to 2 for OuterWall tiles.
        }

        return distance;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Define the possible offsets for 4-way movement (up, down, left, right).
        Vector2Int[] possibleOffsets =
        {
        new Vector2Int(0, 1),  // Up
        new Vector2Int(0, -1), // Down
        new Vector2Int(-1, 0), // Left
        new Vector2Int(1, 0)   // Right
    };

        foreach (Vector2Int offset in possibleOffsets)
        {
            Vector2Int neighborPosition = position + offset;

            // Check if the neighbor is within the grid bounds.
            if (IsWithinGridBounds(neighborPosition))
            {
                // Check if the neighbor is walkable (customize this condition based on your grid).
                if (IsWalkable(neighborPosition))
                {
                    neighbors.Add(neighborPosition);
                }
            }
        }

        return neighbors;
    }

    private bool IsWithinGridBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridSizeX && position.y >= 0 && position.y < gridSizeY;
    }

    private bool IsWalkable(Vector2Int position)
    {
        // Customize this method to check if a cell at the specified position is walkable in your game.
        // For example, you might check if it's not a wall or obstacle.
        // You can use your grid data (the 'grid' variable) to determine walkability.
        if (grid[position.y, position.x] == TileType.Wall)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        // Reconstruct the path from the target node back to the start node.
        List<Vector2Int> path = new List<Vector2Int>();

        while (cameFrom.ContainsKey(current))
        {
            path.Insert(0, current);
            current = cameFrom[current];
        }

        path.Insert(0, current); // Add the start node.

        return path;
    }
}

// PriorityQueue implementation (min-heap)
public class PriorityQueue<T>
{
    private List<Tuple<T, float>> elements = new List<Tuple<T, float>>();

    public int Count { get { return elements.Count; } }

    public bool IsEmpty { get { return elements.Count == 0; } }

    public void Enqueue(T item, float priority)
    {
        elements.Add(new Tuple<T, float>(item, priority));
        int index = elements.Count - 1;

        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;

            if (elements[index].Item2 >= elements[parentIndex].Item2)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    public T Dequeue()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Queue is empty.");

        T frontItem = elements[0].Item1;
        elements[0] = elements[elements.Count - 1];
        elements.RemoveAt(elements.Count - 1);

        int index = 0;

        while (true)
        {
            int leftChildIndex = 2 * index + 1;
            int rightChildIndex = 2 * index + 2;
            int smallestIndex = index;

            if (leftChildIndex < elements.Count && elements[leftChildIndex].Item2 < elements[smallestIndex].Item2)
                smallestIndex = leftChildIndex;

            if (rightChildIndex < elements.Count && elements[rightChildIndex].Item2 < elements[smallestIndex].Item2)
                smallestIndex = rightChildIndex;

            if (smallestIndex == index)
                break;

            Swap(index, smallestIndex);
            index = smallestIndex;
        }

        return frontItem;
    }

    private void Swap(int a, int b)
    {
        Tuple<T, float> temp = elements[a];
        elements[a] = elements[b];
        elements[b] = temp;
    }

    public bool Contains(T item)
    {
        return elements.Any(t => t.Item1.Equals(item));
    }
}

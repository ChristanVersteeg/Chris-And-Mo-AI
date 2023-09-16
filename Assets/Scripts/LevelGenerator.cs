//#define Debug
//#define DelayRoomGeneration

using Random = System.Random;
using UnityEngine;
using Utils;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public enum TileType
{
    Empty,
    Player,
    Enemy,
    Wall,
    OuterWall,
    Door,
    Key,
    Dagger,
    End,
    FakeDoor
}

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] tiles;
    private Random random = new Random();
    private const int gridWidth = 64, gridHeight = gridWidth;
    private const int minRoomSize = 5, maxRoomSizeX = 16, maxRoomSizeY = maxRoomSizeX;
    private const int maxRooms = 25;
    private const int maxRetries = 1000;
    private TileType[,] tileGrid = new TileType[gridWidth, gridHeight];

    private List<GameObject> currentRoom = new();
    private List<Vector4> roomSpaces = new();
    private List<Vector2Int> permanentDoorPositions = new();
    private bool[,] roomGrid = new bool[gridWidth, gridHeight];
    private Vector2 center, size;
    private int cycleCount;

    private Vector2 center1, size1;
    private bool coroutineFinished;
    private List<Vector2> outputCoords = new();

    #region DEBUG
#if Debug
#if DelayRoomGeneration
    [SerializeField] private float time = 0.3f;
    private WaitForSeconds interval = new(0.3f);
#endif

    private List<Vector2Int> debug = new();
    private List<int> intDebug = new();
    private List<int> intDebug2 = new();
    private int right, left, up, down;
    private enum Direction
    {
        Right,
        Left,
        Up,
        Down
    }
#endif
    #endregion

#if !Debug
    private List<Vector2Int> doorPositions = new();
#else
    private int roomCount;
    private List<(Vector2Int, Direction)> doorPositions = new();
#endif

    #region DEBUG
#if DelayRoomGeneration
    private void OnValidate()
    {
        interval = new WaitForSeconds(time);
    }
#endif
    #endregion

    protected void Start()
    {
        StartCoroutine(GenerateRooms());
        StartCoroutine(CheckNearestRoom());
    }

    private Collider2D[] OverLapCheck(int i, int incrementor)
    {
        center1 = new(roomSpaces[i].x + roomSpaces[i].z / 2, roomSpaces[i].y + roomSpaces[i].w / 2);
        size1 = new(roomSpaces[i].z + incrementor, roomSpaces[i].w + incrementor);

        List<Collider2D> doorColliders = new();

        foreach (Collider2D collider in Physics2D.OverlapBoxAll(center1, size1, 0))
            if (collider.transform.name == "FakeDoor(Clone)")
                doorColliders.Add(collider);

        return doorColliders.ToArray();
    }

    private int BaseOverLapCount(int i, int incrementor)
    {
        int baseOverLapCount = 0;
        foreach (Collider2D collider in OverLapCheck(i, incrementor))
            if (collider.transform.parent.name == $"Room{i}")
                baseOverLapCount++;
        return baseOverLapCount;
    }

    private IEnumerator CheckNearestRoom()
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < roomSpaces.Count; i++)
        {
            int incrementor = 0;

            while ((OverLapCheck(i, incrementor).Length - BaseOverLapCount(i, incrementor)) <= 0)
            {
                incrementor++;
                yield return new WaitForSeconds(0.5f);
            }

            List<Collider2D> targetOverlap = OverLapCheck(i, incrementor).Skip(BaseOverLapCount(i, incrementor)).ToList();

            Vector3 sum = Vector3.zero;
            int transformCount = 0;

            foreach (Collider2D collider in targetOverlap)
            {
                sum += collider.transform.position;
                transformCount++;
                outputCoords.Add(collider.transform.position);
            }

            //outputCoords.Add(sum / transformCount);
        }

        foreach (Vector2 vector in outputCoords)
            print(vector);
    }

#if !Debug
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(center1, size1);

        Gizmos.color = Color.yellow;
        foreach (Vector3 vector in outputCoords)
        {
            Gizmos.DrawWireCube(vector, Vector3.one);
        }
    }
#endif

    private IEnumerator GenerateRooms()
    {
        int Random(int val) => random.Next(minRoomSize, val);

        TileType[,] grid = new TileType[gridHeight, gridWidth];

        for (int i = 0; i < maxRooms; i++)
        {
            int x, y, w, h;
            int retryCount = 0;

            void Randomize()
            {
                int IsOdd(int coord)
                {
                    if (coord % 2 == 0) coord += 1;
                    return coord;
                }

                x = Random(gridWidth - maxRoomSizeX);
                y = Random(gridHeight - maxRoomSizeY);
                w = IsOdd(Random(maxRoomSizeX));
                h = IsOdd(Random(maxRoomSizeY));

                center = new Vector2(x + w / 2, y + h / 2);

                size = new Vector2(w, h);

                retryCount++;
            }
            Randomize();

            while (Physics2D.OverlapBoxAll(center, size, 0).Length > 0)
            {
                Randomize();
                if (retryCount > maxRetries) break;
#if DelayRoomGeneration
                yield return interval;
#endif
            }
            if (retryCount > maxRetries) break;

            //Create Rooms
            FillBlock(grid, x, y, w, h, TileType.Wall);

            Vector4 rS = new(x + 1, y + 1, w - 2, h - 2);
            roomSpaces.Add(new(x, y, w, h));
            FillBlock(grid, (int)rS.x, (int)rS.y, (int)rS.z, (int)rS.w, TileType.Empty);

            Vector2Int r = new((int)center.x + w / 2, (int)center.y); //Right tile
            Vector2Int l = new((int)center.x - w / 2, (int)center.y); //Left tile
            Vector2Int u = new((int)center.x, (int)center.y + h / 2); //Up tile
            Vector2Int d = new((int)center.x, (int)center.y - h / 2); //Down tile

#if !Debug
            doorPositions.Add(r);
            doorPositions.Add(l);
            doorPositions.Add(u);
            doorPositions.Add(d);
#else
            doorPositions.Add((r, Direction.Right));
            doorPositions.Add((l, Direction.Left));
            doorPositions.Add((u, Direction.Up));
            doorPositions.Add((d, Direction.Down));

            for (int j = 0; j < doorPositions.Count; j++)
                debug.Add(doorPositions[j].Item1);
#endif

            int openDoors = random.Next(1, 5);
#if Debug
            intDebug.Add(openDoors);
#endif
            int openedDoors = 0;

            void TryOpenDoor()
            {
                if (openedDoors == openDoors) return;

                int rand = random.Next(0, doorPositions.Count);

                //For some reason the y is the first and afterward is the x ¯\_(ツ)_/¯
#if !Debug
                grid[doorPositions[rand].y, doorPositions[rand].x] = TileType.FakeDoor;
#else
                grid[doorPositions[rand].Item1.y, doorPositions[rand].Item1.x] = TileType.Empty;

                switch (doorPositions[rand].Item2)
                {
                    case Direction.Right:
                        right++;
                        break;
                    case Direction.Left:
                        left++;
                        break;
                    case Direction.Up:
                        up++;
                        break;
                    case Direction.Down:
                        down++;
                        break;
                    default:
                        break;
                }
#endif
#if !Debug
                permanentDoorPositions.Add(doorPositions[rand]);
#else
                    permanentDoorPositions.Add(doorPositions[rand].Item1);
#endif
                doorPositions.RemoveAt(rand);

                openedDoors++;
            }

            for (int j = 0; j < openDoors; j++)
                TryOpenDoor();

#if Debug
            intDebug2.Add(openedDoors);
#endif

            CreateTilesFromArray(grid);

            Debugger.instance.AddLabel((int)center.x, (int)center.y, $"Room: {i}").name = $"Room: {i}";

            grid = new TileType[gridHeight, gridWidth];

#if DelayRoomGeneration
            yield return interval;
#else
#if Debug
            roomCount++;
#endif
            doorPositions.Clear();
            yield return null;
#endif
        }

        FillBlock(grid, 32, 28, 1, 1, TileType.Player);
        FillBlock(grid, 30, 30, 1, 1, TileType.Dagger);
        FillBlock(grid, 34, 30, 1, 1, TileType.Key);
        FillBlock(grid, 32, 32, 1, 1, TileType.Door);
        FillBlock(grid, 32, 36, 1, 1, TileType.Enemy);
        FillBlock(grid, 32, 34, 1, 1, TileType.End);

        CreateTilesFromArray(grid);

        grid = new TileType[gridHeight, gridWidth];

        /*for (int y = 0; y < gridHeight; y++) !!!
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (tileGrid[y, x] == TileType.Empty && !roomGrid[y, x])
                    FillBlock(grid, x, y, 1, 1, TileType.OuterWall);
            }
        }*/

        CreateTilesFromArray(grid);

        #region DEBUG
#if Debug
        int totalOpenDoors = 0;
        foreach (int randomInt in intDebug)
            totalOpenDoors += randomInt;

        int actualOpenDoors = 0;
        foreach (int randomInt in intDebug2)
            actualOpenDoors += randomInt;

        float avg = (float)(right + left + up + down) / 4;
        float stDev = StDev(right, left, up, down);

        float StDev(params int[] dirCount)
        {
            float sum = 0;
            for (int i = 0; i < dirCount.Length; i++)
            {
                sum += Mathf.Pow(dirCount[i] - avg, 2) / dirCount.Length;
            }
            return Mathf.Sqrt(sum);
        }
        print($"Open Doors: {totalOpenDoors}, Closed And Open Doors: {roomCount * 4}, Percentage Of All Open Doors: {(float)totalOpenDoors / (roomCount * 4) * 100}%");
        print($"Expected Doors: {totalOpenDoors}, Actual Doors: {actualOpenDoors}");
        print($"Right Doors: {right}, Left Doors: {left}, Up Doors: {up}, Down Doors: {down} \n " +
            $"Average doors per direction {avg}, Standard Deviation: {stDev}, Coefficient of Variation: {stDev / avg}");
#endif
        #endregion

        coroutineFinished = true;
    }

    //fill part of array with tiles 
    private void FillBlock(TileType[,] grid, int x, int y, int width = 1, int height = 1, TileType fillType = TileType.Empty)
    {
        for (int tileY = 0; tileY < height; tileY++)
        {
            for (int tileX = 0; tileX < width; tileX++)
            {
                grid[tileY + y, tileX + x] = fillType;
                tileGrid[tileY + y, tileX + x] = fillType;

                if (grid[tileY + y, tileX + x] != TileType.OuterWall)
                    roomGrid[tileY + y, tileX + x] = true;
            }
        }
    }

    //use array to create tiles
    private void CreateTilesFromArray(TileType[,] grid)
    {
        int height = grid.GetLength(0);
        int width = grid.GetLength(1);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                TileType tile = grid[y, x];
                if (tile != TileType.Empty)
                {
                    CreateTile(x, y, tile);
                }
            }
        }

        Transform parent = new GameObject($"Room{cycleCount}").transform;
        parent.SetParent(transform);

        foreach (GameObject room in currentRoom)
            room.transform.SetParent(parent);

        currentRoom.Clear();
        cycleCount++;
    }

    //create a single tile
    private GameObject CreateTile(int x, int y, TileType type)
    {
        int tileID = ((int)type) - 1;
        if (tileID >= 0 && tileID < tiles.Length)
        {
            GameObject tilePrefab = tiles[tileID];
            if (tilePrefab != null)
            {
                GameObject newTile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                currentRoom.Add(newTile);
                return newTile;
            }
        }
        else
        {
            Debug.LogError("Invalid tile type selected");
        }

        return null;
    }

    #region DEBUG
#if Debug
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                if (tileGrid[i, j] == TileType.Wall)
                    Gizmos.DrawWireCube(new Vector2(j, i), Vector2.one);
            }
        }

        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(center, size);

        foreach (Vector2 vector in debug)
            Gizmos.DrawWireCube(vector, Vector2.one);

        Gizmos.color = Color.cyan;

        foreach (Vector2Int vector in permanentDoorPositions)
            Gizmos.DrawWireCube(new Vector2(vector.x, vector.y), Vector2.one);
    }
#endif
    #endregion
}
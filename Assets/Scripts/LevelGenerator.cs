//#define Debug

using Random = System.Random;
using UnityEngine;
using Utils;
using System.Collections.Generic;
using System.Collections;

public enum TileType
{
    Empty,
    Player,
    Enemy,
    Wall,
    Door,
    Key,
    Dagger,
    End
}

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] tiles;
    private Random random = new Random();
    private const int gridWidth = 64, gridHeight = gridWidth;
    private const int maxRoomSizeX = 8, maxRoomSizeY = maxRoomSizeX;
    private const int maxRooms = 25;
    private Vector2 center, size;
    [SerializeField] private float time = 0.3f;
    private WaitForSeconds interval = new(0.3f);
    private List<GameObject> currentRoom = new();
    private int cycleCount;

    private void OnValidate()
    {
        interval = new WaitForSeconds(time);
    }

    protected void Start()
    {
        StartCoroutine(GenerateRooms());
    }

    private IEnumerator GenerateRooms()
    {
        int Random(int val) => random.Next(3, val);

        TileType[,] grid = new TileType[gridHeight, gridWidth];

        for (int i = 0; i < maxRooms; i++)
        {
            int x, y, w, h;

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
            }
            Randomize();

            while (Physics2D.OverlapBoxAll(center, size, 0).Length > 0)
            {
                Randomize();
#if Debug
                yield return interval;
#endif
            }


            //Create Rooms
            FillBlock(grid, x, y, w, h, TileType.Wall);
            FillBlock(grid, x + 1, y + 1, w - 2, h - 2, TileType.Empty);

            //Create holes in the walls
            CreateTile((int)center.x + w/2, (int)center.y, TileType.Empty); //Right tile
            CreateTile((int)center.x - w/2, (int)center.y, TileType.Empty); //Left tile
            CreateTile((int)center.x, (int)center.y+h/2, TileType.Empty); //Upper tile
            CreateTile((int)center.x, (int)center.y-h/2, TileType.Empty); //Lower tile

            CreateTilesFromArray(grid);

            Debugger.instance.AddLabel((int)center.x, (int)center.y, $"Room: {i}").name = $"Room: {i}";

            grid = new TileType[gridHeight, gridWidth];

#if Debug
            yield return interval;
#else
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
    }

    //fill part of array with tiles 
    private void FillBlock(TileType[,] grid, int x, int y, int width, int height, TileType fillType)
    {
        for (int tileY = 0; tileY < height; tileY++)
        {
            for (int tileX = 0; tileX < width; tileX++)
            {
                grid[tileY + y, tileX + x] = fillType;
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

#if Debug
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(center, size);
    }
#endif
}

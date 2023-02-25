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
    public GameObject[] tiles;
    private const int gridWidth = 64, gridHeight = gridWidth;
    private const int maxRoomSizeX = 8, maxRoomSizeY = maxRoomSizeX;
    private const int maxRooms = 100;
    private Vector2 center, size;
    private WaitForSeconds interval = new(0.1f);


    protected void Start()
    {
        TileType[,] grid = new TileType[gridHeight, gridWidth];

        StartCoroutine(GenerateRooms(grid, maxRooms));
    }

    private IEnumerator GenerateRooms(TileType[,] grid, int roomCount)
    {
        Random rand = new Random();
        int Rand(int val) => rand.Next(3, val);

        for (int i = 0; i < roomCount; i++)
        {
            int x, y, w, h;

            void Randomize()
            {
                x = Rand(gridWidth - maxRoomSizeX);
                y = Rand(gridHeight - maxRoomSizeY);
                w = Rand(maxRoomSizeX);
                h = Rand(maxRoomSizeY);
            }
            Randomize();
            center = new Vector2(x + w / 2, y + h / 2);
            size = new Vector2(w, h);

            while (Physics2D.OverlapBoxAll(new Vector2(x + w / 2, y + h / 2), new Vector2(w, h), 0).Length > 0)
            {
                Randomize();
                center = new Vector2(x + w / 2, y + h / 2);
                size = new Vector2(w, h);
                yield return interval;
            }

            FillBlock(grid, x, y, w, h, TileType.Wall);

            FillBlock(grid, x + 1, y + 1, w - 2, h - 2, TileType.Empty);

            CreateTilesFromArray(grid);

            Debugger.instance.AddLabel(x + w / 2, y + h / 2, $"Room: {i} ");

            yield return interval;
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
                GameObject newTile = GameObject.Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                newTile.transform.SetParent(transform);
                return newTile;
            }

        }
        else
        {
            Debug.LogError("Invalid tile type selected");
        }

        return null;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(center, size);
    }
}

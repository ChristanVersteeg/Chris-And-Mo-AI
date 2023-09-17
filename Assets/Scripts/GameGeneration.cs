using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGeneration : MonoBehaviour
{
    private LevelGenerator gen;

    private void FillAndCreate(int x, int y, TileType tileType) 
    {
        gen.FillBlock(gen.tileGrid, x, y, fillType: tileType);
        gen.CreateTile(x, y, tileType);
    }

    private void GenerateGame() 
    {
        FillAndCreate(32, 28, TileType.Player);
        FillAndCreate(30, 30, TileType.Dagger);
        FillAndCreate(34, 30, TileType.Key);
        FillAndCreate(32, 32, TileType.Door);
        FillAndCreate(32, 36, TileType.Enemy);
        FillAndCreate(32, 34, TileType.End);
    }

    private void Subscribe() => gen.onFinishedPathing += GenerateGame;

    void Start()
    {
        gen = FindObjectOfType<LevelGenerator>();
        Invoke(nameof(Subscribe), 0.5f);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameGeneration : MonoBehaviour
{
    private LevelGenerator gen;
    private float doubleDaggerChance = 0.1f;
    private float enemyModifier = 1.5f;
    public static Action<int> onGameGenerated;

    void Start()
    {
        gen = FindObjectOfType<LevelGenerator>();

        Invoke(nameof(Subscribe), 0.5f);
    }

    private void FillAndCreate(int x, int y, TileType tileType)
    {
        gen.FillBlock(gen.tileGrid, x, y, fillType: tileType);
        gen.CreateTile(x, y, tileType);
    }

    private void RandomiseCoords(in int i, TileType tileType, bool ignoreStartingRoom = false)
    {
        int j = ignoreStartingRoom ? Mathf.Clamp(i, 1, gen.realRoomSpaces.Count) : i;

        int x = Random.Range((int)gen.realRoomSpaces[j].x, 
            (int)(gen.realRoomSpaces[j].x + gen.realRoomSpaces[j].z));

        int y = Random.Range((int)gen.realRoomSpaces[j].y, 
            (int)(gen.realRoomSpaces[j].y + gen.realRoomSpaces[j].w));

        FillAndCreate(x, y, tileType);
    }

    private void GenerateGame()
    {
        RandomiseCoords(0, TileType.Player);
        int keyRoomnum = Random.Range(1, gen.realRoomSpaces.Count);
        int endRoomNum = Random.Range(1, gen.realRoomSpaces.Count);
        if (keyRoomnum == endRoomNum) keyRoomnum = Random.Range(1, gen.realRoomSpaces.Count);
        else RandomiseCoords(keyRoomnum, TileType.Key);

        Debug.Log(keyRoomnum);
        Debug.Log(endRoomNum);
        RandomiseCoords(endRoomNum, TileType.End);

        for (int i = 0; i < Random.Range((int)(gen.realRoomSpaces.Count / enemyModifier), gen.realRoomSpaces.Count); i++)
        {
            RandomiseCoords(i, TileType.Enemy, true);

            for (int j = 0; j < 2; j++)
            {
                RandomiseCoords(i, TileType.Dagger);
                if (Random.value > doubleDaggerChance) break;
            }
        }

        onGameGenerated(endRoomNum);
    }

    private void Subscribe() => gen.onFinishedPathing += GenerateGame;
}

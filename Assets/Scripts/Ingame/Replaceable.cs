using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Replaceable : MonoBehaviour
{
    private LevelGenerator generator;
    private int overlapCount;

    private void CheckOverlap(Vector3 direction) => overlapCount += Physics2D.OverlapBox(transform.position + direction, Vector2.one / 2, 0) == null ? 0 : 1;

    private void ReplacementCheck()
    {
        CheckOverlap(Vector2.left);
        CheckOverlap(Vector2.up);
        CheckOverlap(Vector2.right);
        CheckOverlap(Vector2.down);

        if(overlapCount > 2) 
        { 
            generator.CreateTile((int)transform.position.x, (int)transform.position.y, TileType.Wall);
            Destroy(gameObject);
        }
    }

    void Start()
    {
        generator = FindObjectOfType<LevelGenerator>();
        generator.onFinishedPathing += ReplacementCheck;
    }
}

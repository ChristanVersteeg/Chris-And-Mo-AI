using System.Collections.Generic;
using UnityEngine;

public class Replaceable : MonoBehaviour
{
    private LevelGenerator generator;
    private int overlapCount;
    private List<Vector3> debugList = new();

    private void CheckOverlap(Vector3 direction)
    {
        Collider2D collider = Physics2D.OverlapBox(transform.position + direction, Vector2.one / 2, 0);
        overlapCount += (collider == null) ? 0 : (collider.name == "Wall(Clone)" || collider.name == "OuterWall(Clone)") ? 1 : 0;
        debugList.Add(transform.position + direction);
    }

    private void ReplaceTileWith(TileType tileType)
    {
        generator.CreateTile((int)transform.position.x, (int)transform.position.y, tileType, transform.parent);
        Destroy(gameObject);
    }

    private void ReplacementCheck(int endRoomNum)
    {
        CheckOverlap(Vector2.left);
        CheckOverlap(Vector2.up);
        CheckOverlap(Vector2.right);
        CheckOverlap(Vector2.down);

        if (overlapCount > 2)
            ReplaceTileWith(TileType.Wall);
        else if (transform.parent.name == $"Room{endRoomNum}")
            ReplaceTileWith(TileType.Door);
    }

    void Start()
    {
        generator = FindObjectOfType<LevelGenerator>();
        GameGeneration.onGameGenerated += ReplacementCheck;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.55f, 0.0f);
        foreach (Vector3 vector in debugList)
        {
            Gizmos.DrawWireCube(vector, Vector3.one / 2);
            print(vector);
        }
    }
}

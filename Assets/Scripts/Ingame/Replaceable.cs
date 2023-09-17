using UnityEngine;

public class Replaceable : MonoBehaviour
{
    private LevelGenerator generator;
    private int overlapCount, endRoomNum;

    private void CheckOverlap(Vector3 direction) => overlapCount += Physics2D.OverlapBox(transform.position + direction, Vector2.one / 2, 0) == null ? 0 : 1;

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
}

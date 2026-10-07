using UnityEngine;

public class DirectionChange
{
    public Vector2Int Position;
    public Vector2Int NewDirection;

    public DirectionChange(Vector2Int position, Vector2Int newDirection)
    {
        Position = position;
        NewDirection = newDirection;
    }
}
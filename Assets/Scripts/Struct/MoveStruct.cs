
using UnityEngine;

public struct MovedStruct
{
    public Vector2Int Coordinate;
    public int Score;

    public MovedStruct(Vector2Int coordinate, int score)
    {
        Coordinate = coordinate;
        Score = score;
    }
}
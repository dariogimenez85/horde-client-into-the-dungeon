using UnityEngine;
using System.Collections.Generic;

public static class MovementPlayer
{
    public static List<DirectionChange> DetectDirectionChanges(List<Vector2Int> path, Vector2Int? deadPoint = null)
    {
        List<DirectionChange> changes = new List<DirectionChange>();

        if (path == null || path.Count < 2) return changes;

        // Si hay punto de muerte, recortar el path hasta ese punto (incluido)
        if (deadPoint.HasValue)
        {
            int index = path.IndexOf(deadPoint.Value);
            if (index != -1)
            {
                path = path.GetRange(0, index + 1);
            }
        }

        // ➕ Agregar punto inicial con dirección hacia el segundo
        if (path.Count >= 2)
        {
            Vector2Int firstDirection = path[1] - path[0];
            changes.Add(new DirectionChange(path[0], firstDirection));
        }

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2Int previousDirection = path[i] - path[i - 1];
            Vector2Int nextDirection = path[i + 1] - path[i];

            if (previousDirection != nextDirection)
            {
                changes.Add(new DirectionChange(path[i], nextDirection));
            }
        }

        // ➕ Agregar siempre el punto final con su dirección (desde el penúltimo)
        if (path.Count >= 2)
        {
            Vector2Int finalDirection = path[path.Count - 1] - path[path.Count - 2];
            changes.Add(new DirectionChange(path[path.Count - 1], finalDirection));
        }

        return changes;
    }

}

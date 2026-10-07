using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Controllers;

public class PathBuilder : MonoBehaviour
{
    [Header("References"), Space]
    [SerializeField]
    private GridMatrix gridMatrix;

    [SerializeField]
    private GameParseData gameParseData;

    [SerializeField]
    private MeshPathBuilder meshPathBuilder;

    [SerializeField]
    private GameStateData stateData;

    [SerializeField]
    private Animator mageAnimator;

    private List<Vector2Int> path = new List<Vector2Int>();
    private bool isTracing = false;
    private bool pathComplete = false;
    private bool isGameReady = false;

    private void OnGameStart() => isGameReady = true;
    private void OnGameClose() => ResetPath();

    private void OnEnable()
    {
        GameBridge.OnGameStart += OnGameStart;
        GameBridge.OnGameClosed += OnGameClose;
        GameBridge.OnMoveRejectedByServer += ResetPath;
        GameBridge.OnResyncView += ResetPath;
    }

    private void OnDisable()
    {
        GameBridge.OnGameStart -= OnGameStart;
        GameBridge.OnGameClosed -= OnGameClose;
        GameBridge.OnMoveRejectedByServer -= ResetPath;
        GameBridge.OnResyncView -= ResetPath;
    }

    private void Update()
    {
        // Verificar si hay tween en curso o si el juego está en estado de Game Over
        if (!isGameReady || TweenChecker.IsTweenPlaying(GoblinAnimations.GoblinMove.ToString()) ||
            GameBridge.instance.StagingInfo.GetBool("gameOver")) return;

        if (Input.GetMouseButtonDown(0))
        {
            path.Clear();
            meshPathBuilder.ClearPath();
            Debug.Log("✅ Path start: list cleared");
            isTracing = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isTracing = false;
            mageAnimator.CrossFade("Mage_Idle", 0.15f);
            Debug.Log("🛑 Path end");

            if (!pathComplete)
            {
                path.Clear();
                AudioController.Instance.PlaySound(AudioIds.incompletePath.ToString());
                meshPathBuilder.ClearPath();
                return;
            }
        }

        if (isTracing)
        {
            TraceTileUnderMouse();
        }
    }

    public Vector2Int? GetPlayerStartPosition()
    {
        if (path.Count == 0) return null;
        return path.First();
    }

    private Vector2Int GetTreasurePosition()
    {
        return gameParseData.GetGameTresaurePosition();
    }

    private void TraceTileUnderMouse()
    {
        if (Camera.main == null) return;

        Vector3 mousePos = Input.mousePosition;

        if (!IsValidMousePosition(mousePos)) return;

        if (mousePos.x < 0 || mousePos.y < 0 || mousePos.x > Screen.width || mousePos.y > Screen.height)
            return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mousePos);
        mouseWorld.z = 0;

        if (!IsValidMousePosition(mouseWorld)) return;

        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);
        if (hit == null) return;


        if (!hit.TryGetComponent<TileInfo>(out var tileInfo)) return;

        Vector2Int current = tileInfo.indice;
        Vector2Int playerStart = gameParseData.GetGameInitialPositionPlayer();

        if (path.Count == 0)
        {
            if (current == playerStart)
            {
                path.Add(current);
                AddToPolyline(current);
                Debug.Log($"🟢 Path started at: {current}");
            }
            else
            {
                Debug.Log($"⛔ Invalid start tile: {current}. Expected: {playerStart}");
            }

            return;
        }

        Vector2Int last = path.Last();

        int index = path.IndexOf(current);
        if (index != -1)
        {
            int removedCount = path.Count - (index + 1);
            if (removedCount > 0)
            {
                AudioController.Instance.PlaySound(AudioIds.backCell.ToString());
                path.RemoveRange(index + 1, removedCount);
                meshPathBuilder.ClearPath();
                RedrawPath();
                Debug.Log($"🔁 Partial rollback to {current}. Removed {removedCount} tiles.");
                Debug.Log($"📌 Current path: {string.Join(" → ", path)}");
                return;
            }
        }

        List<Vector2Int> stepPath = FindPathAvoidingTraps(last, current);

        foreach (var step in stepPath)
        {
            if (path.Contains(step)) break;

            if (!IsInsideGrid(step))
                continue;

            TileInfo tile = gridMatrix.GetPosition(step.x, step.y)?.GetComponent<TileInfo>();
            if (tile == null || tile.isTrap)
                continue;


            TileInfo stepTile = gridMatrix.GetPosition(step.x, step.y).GetComponent<TileInfo>();
            if (stepTile == null || stepTile.isTrap)
            {
                Debug.Log($"⛔ Invalid tile or trap at {step}. Path halted.");
                break;
            }

            path.Add(step);
            AddToPolyline(step);
            AudioController.Instance.PlaySound(AudioIds.validCell.ToString());
            Debug.Log($"➕ Tile added: {step}");

            if (step == GetTreasurePosition() && IsValidPath())
            {
                isTracing = false;
                pathComplete = true;
                Debug.Log("🏁 Path complete! Reached the treasure.");
                AudioController.Instance.PlaySound(AudioIds.completePath.ToString());
                SubmitPath();
                break;
            }
            else
            {
                pathComplete = false;
            }
        }

        Debug.Log($"📌 Updated path: {string.Join(" → ", path)}");
    }

    private List<Vector2Int> FindPathAvoidingTraps(Vector2Int start, Vector2Int end)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        // Solo 4 direcciones ortogonales
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == end)
                break;

            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;

                // Validar límites del grid
                if (!IsInsideGrid(neighbor))
                    continue;

                if (visited.Contains(neighbor))
                    continue;

                TileInfo tile = gridMatrix.GetPosition(neighbor.x, neighbor.y)?.GetComponent<TileInfo>();
                if (tile == null || tile.isTrap)
                    continue;

                queue.Enqueue(neighbor);
                visited.Add(neighbor);
                cameFrom[neighbor] = current;
            }
        }

        // Si no se pudo llegar al destino
        if (!cameFrom.ContainsKey(end))
            return new List<Vector2Int>();

        // Reconstruir el camino desde el final al inicio
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int currentStep = end;

        while (currentStep != start)
        {
            path.Add(currentStep);
            currentStep = cameFrom[currentStep];
        }

        path.Reverse();
        return path;
    }

    private bool IsInsideGrid(Vector2Int position)
    {
        // Usá estos valores reales según tu configuración
        int gridWidth = 7; // por ejemplo: 10
        int gridHeight = 7; // por ejemplo: 8

        return position.x >= 0 && position.y >= 0 &&
               position.x < gridWidth && position.y < gridHeight;
    }


    private bool IsOrthogonallyAdjacent(Vector2Int a, Vector2Int b)
    {
        return (a.x == b.x && Mathf.Abs(a.y - b.y) == 1) ||
               (a.y == b.y && Mathf.Abs(a.x - b.x) == 1);
    }

    private bool IsValidMousePosition(Vector3 mp)
    {
        return !(float.IsNaN(mp.x) || float.IsNaN(mp.y) ||
                 float.IsInfinity(mp.x) || float.IsInfinity(mp.y));
    }

    private bool IsValidPath()
    {
        if (path.Count == 0)
            return false;

        Vector2Int? expectedStart = GetPlayerStartPosition();
        Vector2Int actualStart = path.First();
        Vector2Int actualEnd = path.Last();
        Vector2Int treasure = GetTreasurePosition();

        if (actualStart != expectedStart)
        {
            Debug.LogWarning($"⛔ Invalid path start: expected {expectedStart}, got {actualStart}");
            return false;
        }

        if (actualEnd != treasure)
        {
            Debug.LogWarning($"⛔ Path doesn't reach the treasure. Last tile: {actualEnd}, treasure: {treasure}");
            return false;
        }

        for (int i = 0; i < path.Count - 1; i++)
        {
            if (!IsOrthogonallyAdjacent(path[i], path[i + 1]))
            {
                Debug.LogWarning($"⛔ Discontinuous path between {path[i]} and {path[i + 1]}");
                return false;
            }
        }

        return true;
    }

    private void AddToPolyline(Vector2Int index)
    {
        Transform t = gridMatrix.GetPosition(index.x, index.y);
        if (t != null)
            meshPathBuilder.AddPoint(t.position);
    }

    private void RedrawPath()
    {
        foreach (var index in path)
        {
            AddToPolyline(index);
        }
    }

    private void SubmitPath()
    {
        mageAnimator.CrossFade("Mage_Idle", 0.15f);
        string pathString = string.Join(":", path.Skip(1).Select(v => $"{v.x}-{v.y}"));
        GameBridge.instance.HideUIWrapper();
        GameBridge.instance.SendActionToEngine(pathString);

        string snd = "";
        int ramdonSound = Random.Range(0, 2);
        if (ramdonSound == 0)
            snd = AudioIds.goblinTurnSideA.ToString();
        else
            snd = AudioIds.goblinTurnSideB.ToString();

        AudioController.Instance.PlaySound(snd);
    }

    public void ResetPath()
    {
        isTracing = false;
        path.Clear();
        meshPathBuilder.ClearPath();
    }
}
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshPathBuilder : MonoBehaviour
{
    [SerializeField] private Material pathMaterial;
    [SerializeField] private float thickness = 0.5f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private readonly List<Vector3> currentPath = new List<Vector3>();
    private string lastDirection = "Mago_Idle";

    [Header("Magician Animator Reference"), Space]
    [SerializeField] private Animator mageAnimator;

    public enum UVMode { PerSegment, StretchFull }
    public UVMode uvMode = UVMode.PerSegment;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (pathMaterial != null)
            meshRenderer.material = pathMaterial;

        meshRenderer.sortingLayerName = "MaskLigth";
        meshRenderer.sortingOrder = 3;
    }

    private void OnEnable()
    {
        GameBridge.OnGameClosed += ClearPath;
    }

    private void OnDisable()
    {
        GameBridge.OnGameClosed -= ClearPath;
    }

    public void ClearPath()
    {
        currentPath.Clear();
        meshFilter.mesh = new Mesh();
    }

    public void AddPoint(Vector3 point)
    {
        point.z = 0;

        if (currentPath.Count == 0 ||
            Vector3.SqrMagnitude(currentPath[currentPath.Count - 1] - point) > 0.0001f)
        {
            currentPath.Add(point);
            UpdateMesh();
        }
    }

    private void UpdateMesh()
    {
        if (currentPath.Count < 2)
        {
            meshFilter.mesh = null;
            return;
        }

        string pathDirection = GetCurrentDirection();
        SetMageAnimation(pathDirection);

        meshFilter.mesh = GenerateThickPathMesh(currentPath, thickness);
    }

    private Mesh GenerateThickPathMesh(List<Vector3> points, float width)
    {
        if (points.Count < 2)
            return null;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        float halfWidth = width * 0.5f;

        float totalLength = 0f;
        if (uvMode == UVMode.StretchFull)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                float d = Vector3.Distance(points[i], points[i + 1]);
                if (d > Mathf.Epsilon)
                    totalLength += d;
            }

            if (totalLength <= Mathf.Epsilon)
                totalLength = 1f; // protección divide-by-zero
        }

        float currentLength = 0f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 delta = points[i + 1] - points[i];
            float segmentLength = delta.magnitude;

            if (segmentLength <= Mathf.Epsilon)
                continue;

            Vector3 dir = delta / segmentLength; // normalización segura

            float uvStart = uvMode == UVMode.StretchFull ? currentLength / totalLength : 0f;
            float uvEnd = uvMode == UVMode.StretchFull ? (currentLength + segmentLength) / totalLength : 1f;

            GenerateSegment(
                points[i],
                points[i + 1],
                dir,
                halfWidth,
                vertices,
                uvs,
                triangles,
                uvStart,
                uvEnd
            );

            currentLength += segmentLength;
        }

        if (vertices.Count == 0)
            return null;

        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            uv = uvs.ToArray(),
            triangles = triangles.ToArray()
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void GenerateSegment(
        Vector3 start,
        Vector3 end,
        Vector3 direction,
        float halfWidth,
        List<Vector3> vertices,
        List<Vector2> uv,
        List<int> triangles,
        float uvStart,
        float uvEnd)
    {
        Vector3 normal = new Vector3(-direction.y, direction.x, 0f);

        Vector3 startLeft = start - normal * halfWidth;
        Vector3 startRight = start + normal * halfWidth;
        Vector3 endLeft = end - normal * halfWidth;
        Vector3 endRight = end + normal * halfWidth;

        int idx = vertices.Count;

        vertices.Add(startLeft);
        vertices.Add(startRight);
        vertices.Add(endRight);
        vertices.Add(endLeft);

        uv.Add(new Vector2(uvStart, 0));
        uv.Add(new Vector2(uvStart, 1));
        uv.Add(new Vector2(uvEnd, 1));
        uv.Add(new Vector2(uvEnd, 0));

        triangles.Add(idx);
        triangles.Add(idx + 1);
        triangles.Add(idx + 2);
        triangles.Add(idx);
        triangles.Add(idx + 2);
        triangles.Add(idx + 3);
    }

    public string GetCurrentDirection()
    {
        if (currentPath.Count >= 2)
        {
            int last = currentPath.Count - 1;
            int prev = currentPath.Count - 2;

            Vector3 delta = currentPath[last] - currentPath[prev];

            if (delta.sqrMagnitude <= Mathf.Epsilon)
                return lastDirection;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return delta.x > 0 ? "Right" : "Left";
            else
                return delta.y > 0 ? "Up" : "Down";
        }

        return "Undefined";
    }


    private void SetMageAnimation(string direction)
    {
        if (mageAnimator == null || direction == lastDirection)
            return;

        lastDirection = direction;

        const float transitionDuration = 0.15f;

        string targetState = direction switch
        {
            "Right" => "Mago_Right",
            "Left" => "Mago_Left",
            "Up" => "Mago_Up",
            "Down" => "Mago_Down",
            _ => null
        };

        if (string.IsNullOrEmpty(targetState))
            return;

        mageAnimator.CrossFade(
            Animator.StringToHash(targetState),
            transitionDuration,
            0,
            0f
        );
    }
}

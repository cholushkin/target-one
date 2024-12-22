using System.Collections.Generic;
using UnityEngine;

public class SurfaceAnalyzer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Minimum area threshold for the surface to be considered.")]
    public float minSurfaceArea = 1.0f;

    [Tooltip("Tolerance for planarity (in units).")]
    public float planarityTolerance = 0.001f;

    // Struct to store surface information
    public struct SurfaceData
    {
        public GameObject gameObject;
        public Vector3 normal;
        public float area;
        public Vector3 center;

        public SurfaceData(GameObject obj, Vector3 norm, float area, Vector3 center)
        {
            gameObject = obj;
            normal = norm;
            this.area = area;
            this.center = center;
        }
    }

    // HashSet to store the unique surfaces
    public List<SurfaceData> surfaces = new List<SurfaceData>();
    
    
    void Awake()
    {
        AnalyzeSurfaces();
    }

    [ContextMenu("Analyze Surfaces")]
    void AnalyzeSurfaces()
    {
        surfaces.Clear();

        // Get all child objects with MeshFilter components
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            GameObject child = meshFilter.gameObject;
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
                continue;

            AnalyzeMesh(child, mesh);
        }
    }

    public void AnalyzeMesh(GameObject obj, Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] worldVertices = new Vector3[vertices.Length];

        // Convert vertices to world space
        for (int i = 0; i < vertices.Length; i++)
        {
            worldVertices[i] = obj.transform.TransformPoint(vertices[i]);
        }

        // List to keep track of processed triangles
        bool[] processed = new bool[triangles.Length / 3];

        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (processed[i / 3])
                continue;

            List<int> currentSurfaceTriangles = new List<int>();
            Stack<int> triangleStack = new Stack<int>();
            triangleStack.Push(i);

            // Get the normal of the starting triangle
            Vector3 startingNormal = CalculateSurfaceNormal(worldVertices, triangles, i);

            while (triangleStack.Count > 0)
            {
                int tIndex = triangleStack.Pop();
                if (processed[tIndex / 3])
                    continue;

                processed[tIndex / 3] = true;
                currentSurfaceTriangles.Add(tIndex);

                // Check neighboring triangles
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    if (processed[j / 3]) continue;

                    if (AreTrianglesAdjacent(worldVertices, triangles, tIndex, j) &&
                        AreTrianglesCoplanar(worldVertices, triangles, tIndex, j, startingNormal))
                    {
                        triangleStack.Push(j);
                    }
                }
            }

            // Calculate surface area, normal, and center
            if (currentSurfaceTriangles.Count > 0)
            {
                Vector3 normal = startingNormal;
                float area = CalculateSurfaceArea(worldVertices, triangles, currentSurfaceTriangles);
                Vector3 center = CalculateSurfaceCenter(worldVertices, triangles, currentSurfaceTriangles);

                if (area >= minSurfaceArea)
                {
                    surfaces.Add(new SurfaceData(obj, normal, area, center));
                }
            }
        }
    }

    bool AreTrianglesAdjacent(Vector3[] vertices, int[] triangles, int t1, int t2)
    {
        HashSet<int> t1Vertices = new HashSet<int> { triangles[t1], triangles[t1 + 1], triangles[t1 + 2] };
        HashSet<int> t2Vertices = new HashSet<int> { triangles[t2], triangles[t2 + 1], triangles[t2 + 2] };

        int sharedVertices = 0;
        foreach (int v in t1Vertices)
        {
            if (t2Vertices.Contains(v))
            {
                sharedVertices++;
            }
        }

        return sharedVertices >= 2; // They share an edge
    }

    bool AreTrianglesCoplanar(Vector3[] vertices, int[] triangles, int t1, int t2, Vector3 referenceNormal)
    {
        Vector3 normal2 = CalculateSurfaceNormal(vertices, triangles, t2);
        if (Vector3.Angle(referenceNormal, normal2) > 0.1f) return false;

        Vector3 v0 = vertices[triangles[t1]];
        Plane plane = new Plane(referenceNormal, v0);

        for (int i = 0; i < 3; i++)
        {
            Vector3 point = vertices[triangles[t2 + i]];
            if (Mathf.Abs(plane.GetDistanceToPoint(point)) > planarityTolerance)
            {
                return false;
            }
        }

        return true;
    }

    Vector3 CalculateSurfaceNormal(Vector3[] vertices, int[] triangles, int tIndex)
    {
        Vector3 v0 = vertices[triangles[tIndex]];
        Vector3 v1 = vertices[triangles[tIndex + 1]];
        Vector3 v2 = vertices[triangles[tIndex + 2]];

        return Vector3.Cross(v1 - v0, v2 - v0).normalized;
    }

    float CalculateSurfaceArea(Vector3[] vertices, int[] triangles, List<int> surfaceTriangles)
    {
        float totalArea = 0.0f;

        foreach (int tIndex in surfaceTriangles)
        {
            Vector3 v0 = vertices[triangles[tIndex]];
            Vector3 v1 = vertices[triangles[tIndex + 1]];
            Vector3 v2 = vertices[triangles[tIndex + 2]];

            totalArea += Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
        }

        return totalArea;
    }

    Vector3 CalculateSurfaceCenter(Vector3[] vertices, int[] triangles, List<int> surfaceTriangles)
    {
        Vector3 sum = Vector3.zero;
        int totalVertices = 0;

        foreach (int tIndex in surfaceTriangles)
        {
            sum += vertices[triangles[tIndex]];
            sum += vertices[triangles[tIndex + 1]];
            sum += vertices[triangles[tIndex + 2]];
            totalVertices += 3;
        }

        return sum / totalVertices;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var surface in surfaces)
        {
            // Transform the normal to world space
            Vector3 worldNormal = surface.normal;
            // Transform the center to world space
            Vector3 worldCenter = surface.center;
            

            Gizmos.DrawRay(worldCenter, worldNormal);
        }
    }
}

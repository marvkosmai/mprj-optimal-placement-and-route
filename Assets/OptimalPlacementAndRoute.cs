using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptimalPlacementAndRoute : MonoBehaviour
{
    // Samples
    public enum TriangleSelection { PseudoRandom, VanDerCorput }
    [Header("Samples")]
    public int nSamples = 1000;
    public TriangleSelection triangleSelection = TriangleSelection.PseudoRandom;
    public bool samplingStats = false;

    private Mesh mesh;
    private MeshCollider collider;
    private int[] triangles;
    private Vector3[] vertices;

    private List<Vector3> samplePoints = new List<Vector3>();
    private int[] triangleSampleCount;
    private float totalDifference = 0.0f;

    [Space(10)]

    // Gizmo
    [Header("Gizmos")]
    public bool showSamples = true;

    // Start is called before the first frame update
    void Start()
    {
        Init();
        GetSamples();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Draws Gizmos
    private void OnDrawGizmos()
    {
        if (showSamples)
        {
            Gizmos.color = Color.green;
            foreach (Vector3 p in samplePoints)
            {
                Gizmos.DrawSphere(p, 0.1f);
            }
        }
    }
    // --- INIT
    void Init()
    {
        Random.InitState(System.Guid.NewGuid().GetHashCode());

        if (null == mesh)
        {
            mesh = GetComponent<MeshFilter>().mesh;
            triangles = mesh.triangles;
            vertices = mesh.vertices;
        }

        if (null == collider)
        {
            collider = GetComponent<MeshCollider>();
            if (null == collider)
            {
                collider = gameObject.AddComponent<MeshCollider>();
            }
        }
    }

    // INIT ---

    void GenerateGrid()
    {

    }

    // --- SAMPLES
    void GetSamples()
    {
        samplePoints.Clear();

        float[] sizes = GetTriSizes(triangles, vertices);
        float[] cumulativeSizes = new float[sizes.Length];
        float total = 0;

        if (samplingStats)
        {
            triangleSampleCount = new int[sizes.Length];
        }

        for (int i = 0; i < sizes.Length; i++)
        {
            total += sizes[i];
            cumulativeSizes[i] = total;
        }

        for (int i = 0; i < nSamples; i++)
        {
            // make mesh readable in import settings if error
            Vector3 samplePoint = GetRandomPointOnMesh(sizes, cumulativeSizes, total, i);
            samplePoint = collider.transform.localToWorldMatrix.MultiplyPoint(samplePoint);

            samplePoints.Add(samplePoint);
        }

        if (samplingStats)
        {
            Debug.Log($"Samples: {nSamples}");
            Debug.Log($"Triangles: {sizes.Length}");

            totalDifference = 0.0f;
            for (int i = 0; i < sizes.Length; i++)
            {
                float areaPercent = sizes[i] / total;
                float samplePercent = triangleSampleCount[i] / (float) nSamples;

                totalDifference += Mathf.Abs(areaPercent - samplePercent);
            }
            Debug.Log($"Total Difference: {totalDifference}");
        }
    }

    private Vector3 GetRandomPointOnMesh(float[] sizes, float[] cumulativeSizes, float total, int iteration = 0)
    {
        // choose "random" method
        float randomsample = float.MaxValue;
        if (TriangleSelection.PseudoRandom == triangleSelection)
        {
            randomsample = Random.value * total;
        }
        if (TriangleSelection.VanDerCorput == triangleSelection)
        {
            randomsample = VanDerCorputSequence(2, iteration + 1) * total;
        }

        int triIndex = -1;

        for (int i = 0; i < sizes.Length; i++)
        {
            if (randomsample <= cumulativeSizes[i])
            {
                triIndex = i;
                break;
            }
        }

        if (triIndex == -1) Debug.LogError("triIndex should never be -1");

        if (samplingStats)
        {
            triangleSampleCount[triIndex] += 1;
        }

        Vector3 a = vertices[triangles[triIndex * 3]];
        Vector3 b = vertices[triangles[triIndex * 3 + 1]];
        Vector3 c = vertices[triangles[triIndex * 3 + 2]];

        Vector3 norm = (Vector3.Cross((b - a), (c - a))).normalized;

        // generate random barycentric coordinates
        float r = Random.value;
        float s = Random.value;

        if (r + s >= 1)
        {
            r = 1 - r;
            s = 1 - s;
        }

        Vector3 pointOnMesh = a + r * (b - a) + s * (c - a);
        return pointOnMesh + (norm * 0.001f);
    }

    private float[] GetTriSizes(int[] tris, Vector3[] verts)
    {
        int triCount = tris.Length / 3;
        float[] sizes = new float[triCount];
        for (int i = 0; i < triCount; i++)
        {
            sizes[i] = .5f * Vector3.Cross(verts[tris[i * 3 + 1]] - verts[tris[i * 3]], verts[tris[i * 3 + 2]] - verts[tris[i * 3]]).magnitude;
        }
        return sizes;
    }

    private float VanDerCorputSequence(int b, int n)
    {
        float q = 0f;
        float bk = (float) 1/b;

        while (n > 0)
        {
            q += (n % b) * bk;
            n /= b;
            bk /= b;
        }

        return q;
    }

    // only for scientific use
    void GetSamplesData()
    {
        Random.InitState("42".GetHashCode());

        int[] samples = new int[] { 1, 5, 10, 50, 100, 500, 1000, 5000, 10000 };

        float[] pseudoRandom = new float[samples.Length];
        float[] vanDerCorput = new float[samples.Length];

        for (int i = 0; i < samples.Length; i++)
        {
            nSamples = samples[i];
            triangleSelection = TriangleSelection.PseudoRandom;
            GetSamples();
            pseudoRandom[i] = totalDifference;

            triangleSelection = TriangleSelection.VanDerCorput;
            GetSamples();
            vanDerCorput[i] = totalDifference;
        }

        Debug.Log(string.Join(",", samples));
        Debug.Log(string.Join(",", pseudoRandom));
        Debug.Log(string.Join(",", vanDerCorput));
    }

    // SAMPLES ---
}

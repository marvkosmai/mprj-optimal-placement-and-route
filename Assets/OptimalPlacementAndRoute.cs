using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class OptimalPlacementAndRoute : MonoBehaviour
{
    // Samples
    [Header("Samples")]
    public int nSamples = 1000;
    public TriangleSelection triangleSelection = TriangleSelection.PseudoRandom;
    public bool samplingStats = false;

    private Mesh mesh;
    private MeshCollider collider;
    private int[] triangles;
    private Vector3[] vertices;

    private List<SamplePoint> samplePoints = new List<SamplePoint>();
    private int[] triangleSampleCount;
    private float totalDifference = 0.0f;

    [Space(10)]

    // Grid
    [Header("Grid")]
    [Range(0.001f, 10.0f)]
    public float gridSpace = 1.0f;
    [Range(0.0f, 10.0f)]
    public float expandBox = 0.0f;
    [Range(0.0f, 10.0f)]
    public float raiseBox = 0.0f;
    public bool standingOnGround = true;
    public InteractiveMode interactiveMode = InteractiveMode.On;

    private List<Vector3> gridPoints = new List<Vector3>();
    private Bounds originalBounds;
    private Bounds bounds;
    private Vector3 boxTopLeft;

    [Space(10)]

    // EA Pre Computation
    [Header("Pre Computation")]
    [Range(0.01f, 10.0f)]
    public float scanningRange = 3.0f;
    [Range(1.0f, 180.0f)]
    public float maxScanningAngle = 60.0f;

    private List<ComputedGridPoint> computedGridPoints = new List<ComputedGridPoint>();
    private bool gridPointsComputed = false;

    [Space(10)]

    // Placement EA
    [Header("Placement EA")]
    public int size = 200;
    public int positions = 10;
    public Crossover crossover = Crossover.SinglePoint;
    public Selection selection = Selection.Tournament;
    [Range(0.0f, 1.0f)]
    public float mutationRate;

    private Population population;

    [Space(10)]

    // Gizmo
    [Header("Gizmos")]
    public bool showSamples = true;
    public Color samplesColor = Color.green;
    public bool showGridBox = true;
    public bool showGridPoints = true;
    public bool showComputedGridPoint = false;
    [Range(0, 2000)]
    public int showComputedGridPointIndex = 0;
    public bool showBestIndividual = false;

    // Start is called before the first frame update
    void Start()
    {
        Init();
        GetSamples();
        if (interactiveMode == InteractiveMode.Off)
        {
            GenerateGrid();
            ComputeRasterPoints();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (interactiveMode == InteractiveMode.On)
        {
            GenerateGrid();
            return;
        }

        if (!gridPointsComputed)
        {
            ComputeRasterPoints();
        }

        if (!population.isInit())
        {
            population.Init(computedGridPoints, positions);
        }
        
    }

    // Draws Gizmos
    private void OnDrawGizmos()
    {
        if (showSamples && (!gridPointsComputed || !showComputedGridPoint))
        {
            Gizmos.color = samplesColor;
            foreach (SamplePoint p in samplePoints)
            {
                Gizmos.DrawSphere(p.location, 0.1f);
                // Draw normals
                // Gizmos.DrawLine(p.location, p.location + p.normal);
            }
        }

        if (showGridBox)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(
                bounds.center,
                bounds.size
            );
        }

        if (showGridPoints && (!gridPointsComputed || !showComputedGridPoint))
        {
            Gizmos.color = Color.blue;
            foreach (Vector3 p in gridPoints)
            {
                Gizmos.DrawSphere(p, 0.1f);
            }
        }

        if (gridPointsComputed && showComputedGridPoint && (showComputedGridPointIndex < computedGridPoints.Count))
        {
            ComputedGridPoint computedGridPoint = computedGridPoints[showComputedGridPointIndex];

            for (int i = 0; i < nSamples; i++)
            {
                if (computedGridPoint.coverage[i]) Gizmos.color = Color.green;
                else Gizmos.color = Color.red;

                Gizmos.DrawSphere(samplePoints[i].location, 0.1f);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(computedGridPoint.location, 0.2f);
        }

        if (null != population && population.isInit() && showBestIndividual)
        {
            Individual best = population.getBest();

            for (int i = 0; i < nSamples; i++)
            {
                if (best.totalCoverage[i]) Gizmos.color = Color.green;
                else Gizmos.color = Color.red;

                Gizmos.DrawSphere(samplePoints[i].location, 0.1f);
            }

            Gizmos.color = Color.blue;
            foreach (ComputedGridPoint computedGridPoint in best.computedGridPoints)
            {
                Gizmos.DrawSphere(computedGridPoint.location, 0.2f);
            }

        }
    }
    // --- INIT
    void Init()
    {
        Random.InitState(System.Guid.NewGuid().GetHashCode());

        // SAMPLES
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

        // GRID
        if (true)
        {
            originalBounds = GetComponent<Renderer>().bounds;
        }

        // Placement EA
        if (null == population)
        {
            population = new Population(size, selection, crossover);
        }
    }

    // INIT ---

    // --- EA LOCATION PRE COMPUTATION
    void ComputeRasterPoints()
    {
        computedGridPoints.Clear();
        Debug.Log("Computing grid points...");
        foreach (Vector3 gridPoint in gridPoints)
        {
            NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(nSamples, Allocator.TempJob);
            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(nSamples, Allocator.TempJob);

            for (int i = 0; i < nSamples; i++)
            {
                Vector3 origin = samplePoints[i].location;
                Vector3 direction = (gridPoint - origin).normalized;
                commands[i] = new RaycastCommand(origin, direction);
            }

            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, nSamples, default(JobHandle));

            handle.Complete();

            List<bool> hits = new List<bool>();
            for (int i = 0; i < nSamples; i++)
            {
                RaycastHit batchedHit = results[i];

                Vector3 origin = samplePoints[i].location;
                float length = Vector3.Distance(origin, gridPoint);

                float angle = Vector3.Angle(samplePoints[i].normal, gridPoint - origin);

                // only when the ray does not hit a surface,
                // is shorter than the given length
                // and the angle is smaller than the given angle
                // then the sample counts as beeing seen
                hits.Add(
                    batchedHit.collider == null 
                    && length <= scanningRange 
                    && angle <= maxScanningAngle
                    );
            }

            ComputedGridPoint computedGridPoint = new ComputedGridPoint();
            computedGridPoint.location = gridPoint;
            computedGridPoint.coverage = hits;
            computedGridPoints.Add(computedGridPoint);

            results.Dispose();
            commands.Dispose();
        }
        Debug.Log($"{computedGridPoints.Count} grid points computed");

        gridPointsComputed = true;
    }

    // EA LOCATION PRE COMPUTATION ---

    // --- GRID
    void GenerateGrid()
    {
        gridPoints.Clear();

        bounds = originalBounds;
        bounds.Expand(expandBox);
        if (standingOnGround)
        {
            bounds.center = bounds.center + new Vector3(0, expandBox / 2 + raiseBox, 0);
        }
        boxTopLeft = bounds.center + bounds.extents;

        float xOffset = (bounds.size.x % gridSpace) / 2;
        float yOffset = (bounds.size.y % gridSpace) / 2;
        float zOffset = (bounds.size.z % gridSpace) / 2;

        for (int x = 0; x < bounds.size.x / gridSpace; x++)
        {
            for (int y = 0; y < bounds.size.y / gridSpace; y++)
            {
                for (int z = 0; z < bounds.size.z / gridSpace; z++)
                {
                    Vector3 X = new Vector3(gridSpace * x + xOffset, 0, 0);
                    Vector3 Y = new Vector3(0, gridSpace * y + yOffset, 0);
                    Vector3 Z = new Vector3(0, 0, gridSpace * z + zOffset);

                    Vector3 P = boxTopLeft - X - Y - Z;

                    if (!IsInCollider(collider, P))
                    {
                        gridPoints.Add(P);
                    }
                }
            }
        }
    }
    private bool IsInCollider(MeshCollider other, Vector3 point)
    {
        Vector3 from = (Vector3.up * 5000f);
        Vector3 dir = (point - from).normalized;
        float dist = Vector3.Distance(from, point);
        //fwd      
        int hit_count = CastTill(from, point, other);
        //back
        dir = (from - point).normalized;
        hit_count += CastTill(point, point + (dir * dist), other);

        if (hit_count % 2 == 1)
        {
            return true;
        }
        return false;
    }

    int CastTill(Vector3 from, Vector3 to, MeshCollider other)
    {
        int counter = 0;
        Vector3 dir = (to - from).normalized;
        float dist = Vector3.Distance(from, to);
        bool Break = false;
        while (!Break)
        {
            Break = true;
            RaycastHit[] hit = Physics.RaycastAll(from, dir, dist);
            for (int tt = 0; tt < hit.Length; tt++)
            {
                if (hit[tt].collider == other)
                {
                    counter++;
                    from = hit[tt].point + dir.normalized * .001f;
                    dist = Vector3.Distance(from, to);
                    Break = false;
                    break;
                }
            }
        }
        return counter;
    }

    // GRID ---

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
            SamplePoint samplePoint = GetRandomPointOnMesh(sizes, cumulativeSizes, total, i);
            samplePoint.location = collider.transform.localToWorldMatrix.MultiplyPoint(samplePoint.location);
            samplePoint.normal = collider.transform.localToWorldMatrix.MultiplyPoint(samplePoint.normal).normalized;

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

    private SamplePoint GetRandomPointOnMesh(float[] sizes, float[] cumulativeSizes, float total, int iteration = 0)
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
        SamplePoint samplePoint = new SamplePoint();
        samplePoint.location = pointOnMesh + (norm * 0.001f);
        samplePoint.normal = norm;
        return samplePoint;
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

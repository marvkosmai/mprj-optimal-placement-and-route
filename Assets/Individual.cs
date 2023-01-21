using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Individual
{
    public ComputedGridPoint[] computedGridPoints;
    public bool[] totalCoverage;
    public int visibleSamples;
    public int fitness;
    public bool fullyConnected;

    public Individual(List<ComputedGridPoint> gridPoints)
    {
        this.computedGridPoints = new ComputedGridPoint[gridPoints.Count];
        this.totalCoverage = new bool[gridPoints[0].coverage.Count];

        for (int i = 0; i < computedGridPoints.Length; i++)
        {
            ComputedGridPoint gridPoint = gridPoints[i];

            computedGridPoints[i] = gridPoint;

            for (int j = 0; j < totalCoverage.Length; j++)
            {
                if (gridPoint.coverage[j])
                {
                    totalCoverage[j] = true;
                }
            }
        }

        this.CalcFitness();
    }

    private void CalcFitness()
    {
        int samples = 0;
        for (int j = 0; j < totalCoverage.Length; j++)
        {
            if (totalCoverage[j])
            {
                samples++;
            }
        }

        CalcFullyConnected();

        this.visibleSamples = samples;

        this.fitness = visibleSamples;
    }

    private void CalcFullyConnected()
    {
        List<ComputedGridPoint> visited = new List<ComputedGridPoint>();

        Queue<ComputedGridPoint> q = new Queue<ComputedGridPoint>();
        q.Enqueue(computedGridPoints[0]);

        visited.Add(computedGridPoints[0]);

        while (q.Count != 0)
        {
            ComputedGridPoint current = q.Dequeue();

            foreach (ComputedGridPoint cgp in computedGridPoints)
            {
                if (visited.Contains(cgp))
                {
                    continue;
                }

                RaycastHit hit;
                if (!Physics.Raycast(current.location, (cgp.location - current.location).normalized, out hit, Mathf.Infinity))
                {
                    q.Enqueue(cgp);
                    visited.Add(cgp);
                }
            }
        }

        fullyConnected = visited.Count == computedGridPoints.Length;
    }

}

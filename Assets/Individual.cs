using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Individual
{
    public List<ComputedGridPoint> allComputedGridPoints;
    public bool[] chromosomeGridPoints;
    public ComputedGridPoint[] computedGridPoints;
    public bool[] totalCoverage;
    public int visibleSamples;
    public int fitness;
    public bool fullyConnected;

    public Individual(List<ComputedGridPoint> gridPoints, int initalPositions)
    {
        this.allComputedGridPoints = gridPoints;
        this.chromosomeGridPoints = new bool[gridPoints.Count];
        

        for (int i = 0; i < initalPositions; i++)
        {
            int randomIndex = Random.Range(0, gridPoints.Count);
            this.chromosomeGridPoints[randomIndex] = true;
        }
    }

    public Individual(List<ComputedGridPoint> gridPoints, bool[] chromosomeGridPoints)
    {
        this.allComputedGridPoints = gridPoints;
        this.chromosomeGridPoints = chromosomeGridPoints;
    }

    public void Init()
    {
        this.totalCoverage = new bool[allComputedGridPoints[0].coverage.Count];

        int positions = 0;
        for (int i = 0; i < chromosomeGridPoints.Length; i++)
        {
            if (chromosomeGridPoints[i])
            {
                positions++;
            }
        }
        this.computedGridPoints = new ComputedGridPoint[positions];

        int count = 0;
        for (int i = 0; i < chromosomeGridPoints.Length; i++)
        {
            if (chromosomeGridPoints[i])
            {
                computedGridPoints[count++] = allComputedGridPoints[i];
            }
        }

        for (int i = 0; i < computedGridPoints.Length; i++)
        {
            ComputedGridPoint gridPoint = computedGridPoints[i];

            for (int j = 0; j < totalCoverage.Length; j++)
            {
                if (gridPoint.coverage[j])
                {
                    totalCoverage[j] = true;
                }
            }
        }
    }

    public void CalcFitness()
    {
        if (computedGridPoints.Length == 0)
        {
            this.fitness = 1;
            return;
        }

        int samples = 0;
        for (int j = 0; j < totalCoverage.Length; j++)
        {
            if (totalCoverage[j])
            {
                samples++;
            }
        }

        //CalcFullyConnected();

        this.visibleSamples = samples;

        float visiblePercent = ((float)visibleSamples / this.allComputedGridPoints[0].coverage.Count) * 100;
        float locationPercent = ((float)computedGridPoints.Length / this.allComputedGridPoints.Count) * 100;

        this.fitness = (int)(0.2 * visiblePercent) + (int)(0.8 * (100.0 - locationPercent));
    }

    // Implemted a BTS
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

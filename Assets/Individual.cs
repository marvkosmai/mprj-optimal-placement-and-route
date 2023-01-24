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
        this.chromosomeGridPoints = new bool[gridPoints.Count];
        

        for (int i = 0; i < initalPositions; i++)
        {
            int randomIndex = Random.Range(0, gridPoints.Count);
            this.chromosomeGridPoints[randomIndex] = true;
        }

        this.Init(gridPoints);
    }

    public Individual(List<ComputedGridPoint> gridPoints, bool[] chromosomeGridPoints)
    {
        this.chromosomeGridPoints = chromosomeGridPoints;

        this.Init(gridPoints);
    }

    public void Init(List<ComputedGridPoint> gridPoints)
    {
        this.allComputedGridPoints = gridPoints;
        this.totalCoverage = new bool[gridPoints[0].coverage.Count];

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
                computedGridPoints[count++] = gridPoints[i];
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

        this.CalcFitness();
    }

    private void CalcFitness()
    {
        if (computedGridPoints.Length == 0)
        {
            this.fitness = 0;
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

        float visiblePercent = (((float)visibleSamples / this.allComputedGridPoints[0].coverage.Count) * 100);
        float locationPercent = (((float)computedGridPoints.Length / this.allComputedGridPoints.Count) * 100);

        this.fitness = (int)((visiblePercent / locationPercent) * 100);
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

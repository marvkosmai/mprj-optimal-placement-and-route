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
    public int possibleVisibleSamples;
    public float fitness = 0.0f;
    public bool fullyConnected;

    public Individual(List<ComputedGridPoint> gridPoints, int initalPositions, int possibleVisibleSamples)
    {
        this.allComputedGridPoints = gridPoints;
        this.chromosomeGridPoints = new bool[gridPoints.Count];
        this.possibleVisibleSamples = possibleVisibleSamples;
        

        for (int i = 0; i < initalPositions; i++)
        {
            int randomIndex = Random.Range(0, gridPoints.Count);
            this.chromosomeGridPoints[randomIndex] = true;
        }
    }

    public Individual(List<ComputedGridPoint> gridPoints, bool[] chromosomeGridPoints, int possibleVisibleSamples)
    {
        this.allComputedGridPoints = gridPoints;
        this.chromosomeGridPoints = chromosomeGridPoints;
        this.possibleVisibleSamples = possibleVisibleSamples;
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
            this.fitness = 0.01f;
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

        //float visiblePercent = (float)visibleSamples / this.allComputedGridPoints[0].coverage.Count;
        float visiblePercent = (float)visibleSamples / possibleVisibleSamples;
        float locationPercent = (float)computedGridPoints.Length / this.allComputedGridPoints.Count;

        float d1 = 0.5f;
        float d2 = 0.5f;

        //this.fitness = visiblePercent +  (Mathf.Pow(2, 1.0f - locationPercent) - 1);
        this.fitness = (d1 * visiblePercent) + (d2 * Mathf.Pow(1.0f - locationPercent, 3));
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

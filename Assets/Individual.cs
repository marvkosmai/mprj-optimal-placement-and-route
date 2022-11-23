using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Individual
{
    public ComputedGridPoint[] computedGridPoints;
    public bool[] totalCoverage;
    public int fitness;

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

        this.calcFitness();
    }

    private void calcFitness()
    {
        int f = 0;
        for (int j = 0; j < totalCoverage.Length; j++)
        {
            if (totalCoverage[j])
            {
                f++;
            }
        }

        this.fitness = f;
    }

}

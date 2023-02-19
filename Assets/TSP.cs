using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSP
{
    private Individual individual;

    private ComputedGridPoint[] computedGridPoints;

    private float[,] map;

    private int populationSize = 200;
    private float mutationRate = 0.01f;
    private int generation;

    private List<Route> population;

    public bool init = false;

    public void SetIndividual(Individual individual)
    {
        this.individual = individual;
    }

    public void Init()
    {
        if (null == individual)
        {
            throw new Exception("no individual");
        }

        // calculate distance map
        computedGridPoints = individual.computedGridPoints;
        int n = computedGridPoints.Length;

        map = new float[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    map[i, j] = 0f;
                    continue;
                }

                ComputedGridPoint a = computedGridPoints[i];
                ComputedGridPoint b = computedGridPoints[j];

                float distance = float.MaxValue;

                RaycastHit hit;
                if (!Physics.Raycast(a.location, (b.location - a.location).normalized, out hit, Mathf.Infinity))
                {
                    distance = (b.location - a.location).magnitude;
                }

                map[i, j] = distance;
                map[j, i] = distance;
            }
        }

        System.Random rnd = new System.Random();

        // initialize population;
        population = new List<Route>();

        for (int i = 0; i < populationSize; i++)
        {
            int[] route = new int[n];
            for (int j = 0; j < n; j++)
            {
                route[j] = j;
            }

            rnd.Shuffle(route);
            Route individual = new Route(map, route);
            individual.CalcFitness();

            population.Add(individual);
        }

        Sort();

        init = true;
    }

    public void Iterate()
    {

    }

    public Route Best()
    {
        return population[0];
    }

    private void Sort()
    {
        population.Sort((a, b) => a.fitness.CompareTo(b.fitness));
    }

    public bool IsInit()
    {
        return init;
    }
}

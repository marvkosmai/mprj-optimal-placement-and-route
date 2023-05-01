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
    private float mutationRate = 0.05f;
    private int generation;

    private List<Route> population;

    public bool init = false;

    public float averageFitness;
    public int counter;

    private Route best;

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


        averageFitness = float.MaxValue;
        counter = 0;

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

                float distance = -1.0f;

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
        // Selection
        List<Route> pool = new List<Route>();
        for (int i = 0; i < populationSize; i += 2)
        {
            pool.Add(TournamentSelection());
        }

        // Recombination
        List<Route> newPopulation = new List<Route>();
        for (int i = 0; i < populationSize; i++)
        {
            Route offspring = EdgeRecombination(
                pool[UnityEngine.Random.Range(0, pool.Count)],
                pool[UnityEngine.Random.Range(0, pool.Count)]
            );

            offspring = InversionMutation(offspring);

            newPopulation.Add(offspring);
        }
        population.Clear();
        population = newPopulation;
        Sort();

        if (Best().fitness > population[0].fitness)
        {
            best = population[0];
        }

        if (averageFitness > getAverageFitness())
        {
            averageFitness = getAverageFitness();
            counter = 0;
        } else
        {
            counter++;
        }

        if (counter >= 1000)
        {
            Debug.Log(best.errors);
            Debug.Log(best.fitness);
            init = false;
            best = null;
        }
    }

    private float getAverageFitness()
    {
        float f = 0;

        foreach (Route r in population)
        {
            f += r.fitness;
        }

        return f / population.Count;
    }

    private Route InversionMutation(Route offspring)
    {
        if (UnityEngine.Random.Range(0.0f, 1.0f) > mutationRate)
        {
            return offspring;
        }

        int start = UnityEngine.Random.Range(0, offspring.route.Length);
        int end = UnityEngine.Random.Range(0, offspring.route.Length);

        if (start == end) return offspring;

        if (start > end)
        {
            int temp = start;
            start = end;
            end = temp;
        }

        int[] route = offspring.route;
        int[] routeCopy = offspring.route.Clone() as int[];

        int step = end;
        for (int i = start; i <= end; i++)
        {
            route[i] = routeCopy[step];
            step--;
        }

        offspring = new Route(map, route);
        offspring.CalcFitness();

        return offspring;
    }

    private Route TournamentSelection()
    {
        Route a = population[UnityEngine.Random.Range(0, population.Count)];
        Route b = population[UnityEngine.Random.Range(0, population.Count)];

        return a.fitness < b.fitness ? a : b;
    }

    private Route EdgeRecombination(Route a, Route b)
    {
        List<List<int>> edgeMap = new List<List<int>>();

        for (int i = 0; i < a.route.Length; i++)
        {
            edgeMap.Add(new List<int>());
        }

        for (int i = 0; i < a.route.Length; i++)
        {
            List<int> edges = edgeMap[a.route[i]];
            int before = i - 1;
            int after = i + 1;

            if (-1 == before) before = a.route.Length - 1;
            if (a.route.Length == after) after = 0;

            if (!edges.Contains(a.route[before])) edges.Add(a.route[before]);
            if (!edges.Contains(a.route[after])) edges.Add(a.route[after]);

            edgeMap[a.route[i]] = edges;
        }

        for (int i = 0; i < b.route.Length; i++)
        {
            List<int> edges = edgeMap[b.route[i]];
            int before = i - 1;
            int after = i + 1;

            if (-1 == before) before = b.route.Length - 1;
            if (b.route.Length == after) after = 0;

            if (!edges.Contains(b.route[before])) edges.Add(b.route[before]);
            if (!edges.Contains(b.route[after])) edges.Add(b.route[after]);

            edgeMap[b.route[i]] = edges;
        }

        int[] route = new int[a.route.Length];

        // choose starting location
        int location = LocationWithLessEdges(a.route[0], b.route[0], edgeMap);

        for (int i = 0; i < a.route.Length; i++)
        {
            route[i] = location;
            edgeMap = RemoveLocationFromMap(location, edgeMap);

            if (i == a.route.Length - 1)
            {
                continue;
            }

            List<int> edges = edgeMap[location];

            if (0 == edges.Count)
            {
                location = RandomLocation(edgeMap, route, i);
                continue;
            }
            
            int next = edges[0];

            for (int j = 1; j < edges.Count; j++)
            {
                next = LocationWithLessEdges(next, edges[j], edgeMap);
            }

            location = next;
        }

        Route offspring = new Route(map, route);
        offspring.CalcFitness();

        return offspring;
    }

    private int RandomLocation(List<List<int>> edgeMap, int[] visited, int length)
    {
        List<int> unvisited = new List<int>();
        for (int i = 0; i < edgeMap.Count; i++)
        {
            bool next = false;
            for (int j = 0; j <= length; j++)
            {
                if (i == visited[j]) next = true;
            }

            if (next) continue;

            unvisited.Add(i);
        }

        return unvisited[UnityEngine.Random.Range(0, unvisited.Count)];
    }

    private int LocationWithLessEdges(int a, int b, List<List<int>> edgeMap)
    {
        List<int> edgesA = edgeMap[a];
        List<int> edgesB = edgeMap[b];

        if (edgesA.Count == edgesB.Count)
        {
            return UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f ? a : b;
        }

        return edgesA.Count < edgesB.Count ? a : b;
    }

    private List<List<int>> RemoveLocationFromMap(int location, List<List<int>> edgeMap)
    {
        for (int i = 0; i < edgeMap.Count; i++)
        {
            List<int> edgeList = edgeMap[i];
            edgeList.Remove(location);
            edgeMap[i] = edgeList;
        }

        return edgeMap;
    }

    public Route Best()
    {
        if (null == best)
        {
            best = population[0];
        }
        return best;
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

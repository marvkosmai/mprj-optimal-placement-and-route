using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Population
{

    private List<Individual> individuals;

    private Selection selection;
    private Crossover crossover;

    private int size;
    private bool init;

    public Population(int size, Selection selection, Crossover crossover)
    {
        this.size = size;
        individuals = new List<Individual>();
        this.selection = selection;
        this.crossover = crossover;

        this.init = false;
    }

    public void Init(List<ComputedGridPoint> computedGridPoints, int positions)
    {
        individuals.Clear();
        for (int i = 0; i < this.size; i++)
        {
            List<ComputedGridPoint> randomGridPoints = new List<ComputedGridPoint>();
            for (int j = 0; j < positions; j++)
            {
                randomGridPoints.Add(computedGridPoints[Random.Range(0, computedGridPoints.Count)]);
            }
            individuals.Add(new Individual(randomGridPoints));
        }

        this.Sort();

        Debug.Log("Population initialized!");
        this.init = true;
    }

    public bool isInit()
    {
        return this.init;
    }

    public Individual getBest()
    {
        return individuals[0];
    }

    private void Sort()
    {
        individuals.Sort((a, b) => b.fitness.CompareTo(a.fitness));
    }
}

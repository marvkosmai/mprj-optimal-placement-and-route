using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Population
{
    private List<Individual> individuals;
    private List<ComputedGridPoint> computedGridPoints;

    private Selection selection;
    private Crossover crossover;

    private int size;
    private float mutationRate;
    private float elits;
    private bool init;

    public Population(int size, Selection selection, Crossover crossover, float mutationRate, float elits)
    {
        this.size = size;
        individuals = new List<Individual>();
        this.selection = selection;
        this.crossover = crossover;
        this.mutationRate = mutationRate;
        this.elits = elits;

        this.init = false;
    }

    public void Init(List<ComputedGridPoint> computedGridPoints, int positions)
    {
        this.computedGridPoints = computedGridPoints;

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

    public void Iterate()
    {
        List<Individual> newIndividuals = new List<Individual>();

        int elits = (int) (size * this.elits);
        for (int i = 0; i < elits; i++)
        {
            newIndividuals.Add(new Individual(new List<ComputedGridPoint>(individuals[i].computedGridPoints)));
        }

        for (int i = elits; i < size; i++) 
        {
            Individual kid = PointCrossover(Tournament(), Tournament());

            kid = Mutate(kid);

            newIndividuals.Add(kid);
        }

        individuals.Clear();
        individuals = newIndividuals;
        Sort();
    }

    private Individual Tournament()
    {
        Individual a = individuals[Random.Range(0, individuals.Count)];
        Individual b = individuals[Random.Range(0, individuals.Count)];

        return a.fitness > b.fitness ? a : b;
    }

    private Individual PointCrossover(Individual a, Individual b)
    {
        List<ComputedGridPoint> computedGridPoints = new List<ComputedGridPoint>();
        int length = a.computedGridPoints.Length;
        int crossover = Random.Range(0, length);

        for (int i = 0; i < length; i++)
        {
            if (i < crossover)
            {
                computedGridPoints.Add(a.computedGridPoints[i]);
            } else
            {
                computedGridPoints.Add(b.computedGridPoints[i]);
            }
        }

        return new Individual(computedGridPoints);
    }

    private Individual Mutate(Individual individual)
    {
        if (Random.Range(0.0f, 1.0f) <= mutationRate)
        {
            return individual;
        }

        int point = Random.Range(0, individual.computedGridPoints.Length);
        individual.computedGridPoints[point] = computedGridPoints[Random.Range(0, computedGridPoints.Count)];

        return individual;
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

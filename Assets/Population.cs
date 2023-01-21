using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Population
{
    private List<Individual> individuals;
    private List<ComputedGridPoint> computedGridPoints;

    private Selection selection;
    private Crossover crossover;

    private int nSamples;

    private int size;
    private float mutationRate;
    private float elits;
    private bool init;

    private int generation;

    private PopulationStats stats;

    public Population(int size, Selection selection, Crossover crossover, float mutationRate, float elits, int nSamples)
    {
        this.size = size;
        individuals = new List<Individual>();
        this.selection = selection;
        this.crossover = crossover;
        this.mutationRate = mutationRate;
        this.elits = elits;

        this.nSamples = nSamples;

        this.generation = 0;
        this.init = false;

        stats = new PopulationStats();
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
        this.generation = 1;
        addStats();
    }

    public void Iterate()
    {
        List<Individual> newIndividuals = new List<Individual>();

        int elits = (int) (size * this.elits);
        for (int i = 0; i < elits; i++)
        {
            newIndividuals.Add(new Individual(new List<ComputedGridPoint>(individuals[i].computedGridPoints)));
        }

        List<Individual> matingPool = new List<Individual>();

        for (int i = 0; i < size; i++)
        {
            if (selection == Selection.Tournament)
            {
                matingPool.Add(Tournament());
            } 
            else if (selection == Selection.RouletteWheel)
            {
                matingPool.Add(RouletteWheel());
            }
            else
            {
                throw new System.NotImplementedException("No selection method!");
            }
        }

        for (int i = elits; i < size; i++) 
        {
            Individual kid;
            if (crossover == Crossover.SinglePoint)
            {
                kid = PointCrossover(
                    matingPool[Random.Range(0, matingPool.Count)],
                    matingPool[Random.Range(0, matingPool.Count)]
                );
            } 
            else
            {
                throw new System.NotImplementedException("No crossover method!");
            }
            

            kid = Mutate(kid);

            newIndividuals.Add(kid);
        }

        individuals.Clear();
        individuals = newIndividuals;
        Sort();
        generation++;

        addStats();
    }

    private Individual Tournament()
    {
        Individual a = individuals[Random.Range(0, individuals.Count)];
        Individual b = individuals[Random.Range(0, individuals.Count)];

        return a.fitness > b.fitness ? a : b;
    }

    private Individual RouletteWheel()
    {
        int totalFitness = 0;
        List<int> sections = new List<int>();

        foreach (Individual individual in individuals)
        {
            totalFitness += individual.fitness;
            sections.Add(totalFitness);
        }

        int selection = Random.Range(0, totalFitness);
        int index = -1;
        for (int i = 0; i < sections.Count; i++)
        {
            if (selection <= sections[i])
            {
                index = i;
                break;
            }
        }

        return individuals[index];
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

    private void addStats()
    {
        stats.addBestVisibility(getBestVisibility());
    }

    public Individual getBest()
    {
        return individuals[0];
    }

    public float getAverageFitness()
    {
        int totalFitness = 0;

        foreach (Individual i in individuals)
        {
            totalFitness += i.fitness;
        }

        return totalFitness / size;
    }

    public float getBestVisibility()
    {
        return getBest().visibleSamples / (float) nSamples;
    }

    public float getStandardDeviation() 
    {
        float mean = getAverageFitness();

        float sum = 0;
        foreach (Individual i in individuals)
        {
            sum += (i.fitness - mean) * (i.fitness - mean);
        }

        return Mathf.Sqrt(sum / size);
    }

    public void printStats(int everyGeneration = 10)
    {
        if (generation % everyGeneration != 0)
        {
            return;
        }

        Debug.Log(
            $"Generation: {generation} | " +
            $"Best Visibility: {getBestVisibility()} | " +
            $"Average Fitness: {getAverageFitness()} | " +
            $"Standard Deviation: {getStandardDeviation()}"
        );
    }

    private void Sort()
    {
        individuals.Sort((a, b) => b.fitness.CompareTo(a.fitness));
    }
}

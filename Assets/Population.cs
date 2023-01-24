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
    private float crossoverRate;
    private float elits;
    private bool init;

    private int generation;

    private PopulationStats stats;

    public Population(int size, Selection selection, Crossover crossover, float mutationRate, float crossoverRate, float elits, int nSamples)
    {
        this.size = size;
        individuals = new List<Individual>();
        this.selection = selection;
        this.crossover = crossover;
        this.mutationRate = mutationRate;
        this.crossoverRate = crossoverRate;
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
            individuals.Add(new Individual(computedGridPoints, positions));
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
            newIndividuals.Add(new Individual(computedGridPoints, individuals[i].chromosomeGridPoints));
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
        int length = a.chromosomeGridPoints.Length;
        bool[] chromosomeGridPoints = new bool[length];
        int crossover = Random.Range(0, length);

        if (Random.Range(0.0f, 1.0f) > crossoverRate)
        {
            return a;
        }

        for (int i = 0; i < length; i++)
        {
            if (i < crossover)
            {
                chromosomeGridPoints[i] = a.chromosomeGridPoints[i];
            } 
            else
            {
                chromosomeGridPoints[i] = b.chromosomeGridPoints[i];
            }
        }

        return new Individual(computedGridPoints, chromosomeGridPoints);
    }

    private Individual Mutate(Individual individual)
    {
        /**
        for (int i = 0; i < individual.chromosomeGridPoints.Length; i++)
        {
            if (individual.chromosomeGridPoints[i] && Random.Range(0.0f, 1.0f) > mutationRate)
            {
                individual.chromosomeGridPoints[i] = false;
            }
        }
        **/

        
        if (Random.Range(0.0f, 1.0f) <= mutationRate)
        {
            return individual;
        }
        int point = Random.Range(0, individual.chromosomeGridPoints.Length);
        individual.chromosomeGridPoints[point] = !individual.chromosomeGridPoints[point];
        
        individual.Init(computedGridPoints);

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

    public float getAverageLocations()
    {
        int totalLocations = 0;

        foreach (Individual i in individuals)
        {
            totalLocations += i.computedGridPoints.Length;
        }

        return totalLocations / size;
    }

    public float getBestVisibility()
    {
        return getBest().visibleSamples / (float) nSamples;
    }

    public int getBestFitness()
    {
        return getBest().fitness;
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
            $"Best Visibility: {getBestVisibility()} {getBest().fullyConnected} | " +
            $"Best Fitness: {getBestFitness()} | " +
            $"Avergae Locations: {getAverageLocations()} | " +
            $"Average Fitness: {getAverageFitness()} | " +
            $"Standard Deviation: {getStandardDeviation()}"
        );
    }

    private void Sort()
    {
        individuals.Sort((a, b) => b.fitness.CompareTo(a.fitness));
    }
}

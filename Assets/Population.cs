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
    private int visiableSamples;

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
        this.visiableSamples = calcVisibleSamples();

        Debug.Log($"Samples: {nSamples}");
        Debug.Log($"Visible Samples: {visiableSamples}");

        individuals.Clear();
        for (int i = 0; i < this.size; i++)
        {
            Individual individual = new Individual(computedGridPoints, positions, visiableSamples);
            individual.Init();
            individual.CalcFitness();
            individuals.Add(individual);
        }

        this.Sort();

        Debug.Log("Population initialized!");
        this.init = true;
        this.generation = 1;
        addStats();
    }

    private int calcVisibleSamples()
    {
        bool[] totalCoverage = new bool[computedGridPoints[0].coverage.Count];

        for (int i = 0; i < computedGridPoints.Count; i++)
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

        int count = 0;
        foreach (bool visible in totalCoverage)
        {
            if (visible) count++;
        }

        return count;
    }

    public void Iterate()
    {
        List<Individual> newIndividuals = new List<Individual>();

        int elits = (int) (size * this.elits);
        if ((size - elits) % 2 == 1)
        {
            elits--;
        }
        for (int i = 0; i < elits; i++)
        {
            Individual elit = new Individual(computedGridPoints, individuals[i].chromosomeGridPoints, visiableSamples);
            elit.Init();
            elit.CalcFitness();
            newIndividuals.Add(elit);
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

        for (int i = elits; i < size; i += 2) 
        {
            Individual[] kids;
            if (crossover == Crossover.SinglePoint)
            {
                kids = PointCrossover(
                    matingPool[Random.Range(0, matingPool.Count)],
                    matingPool[Random.Range(0, matingPool.Count)]
                );
            } 
            else
            {
                throw new System.NotImplementedException("No crossover method!");
            }
            

            Individual kidA = Mutate(kids[0]);
            Individual kidB = Mutate(kids[1]);

            kidA.Init();
            kidB.Init();
            kidA.CalcFitness();
            kidB.CalcFitness();

            newIndividuals.Add(kidA);
            newIndividuals.Add(kidB);
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
        float totalFitness = 0f;
        List<float> sections = new List<float>();

        foreach (Individual individual in individuals)
        {
            totalFitness += individual.fitness;
            sections.Add(totalFitness);
        }

        float selection = Random.Range(0f, totalFitness);
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

    private Individual[] PointCrossover(Individual a, Individual b)
    {
        int length = a.chromosomeGridPoints.Length;
        bool[] chromosomeGridPointsA = new bool[length];
        bool[] chromosomeGridPointsB = new bool[length];
        int crossover = Random.Range(0, length);

        if (Random.Range(0.0f, 1.0f) > crossoverRate)
        {
            return new Individual[] { a, b };
        }

        for (int i = 0; i < length; i++)
        {
            if (i < crossover)
            {
                chromosomeGridPointsA[i] = a.chromosomeGridPoints[i];
                chromosomeGridPointsB[i] = b.chromosomeGridPoints[i];
            } 
            else
            {
                chromosomeGridPointsA[i] = b.chromosomeGridPoints[i];
                chromosomeGridPointsB[i] = a.chromosomeGridPoints[i];
            }
        }

        return new Individual[]
        {
            new Individual(computedGridPoints, chromosomeGridPointsA, visiableSamples),
            new Individual(computedGridPoints, chromosomeGridPointsB, visiableSamples)
        };
    }

    private Individual Mutate(Individual individual)
    {
        if (Random.Range(0.0f, 1.0f) <= mutationRate)
        {
            return individual;
        }
        
        List<int> active = new List<int>();
        List<int> inactive = new List<int>();
        for (int i = 0; i < individual.chromosomeGridPoints.Length; i++)
        {
            if (individual.chromosomeGridPoints[i])
            {
                active.Add(i);
            }
            else
            {
                inactive.Add(i);
            }
        }

        if (active.Count > 0 && inactive.Count == 0)
        {
            int p = active[Random.Range(0, active.Count)];
            individual.chromosomeGridPoints[p] = false;
            return individual;
        }

        if (inactive.Count > 0 && active.Count == 0)
        {
            int p = inactive[Random.Range(0, inactive.Count)];
            individual.chromosomeGridPoints[p] = true;
            return individual;
        }

        if (Random.Range(0.0f, 1.0f) <= 0.5)
        {
            int p = active[Random.Range(0, active.Count)];
            individual.chromosomeGridPoints[p] = false;
        }
        else
        {
            int p = inactive[Random.Range(0, inactive.Count)];
            individual.chromosomeGridPoints[p] = true;
        }

        return individual;
    }

    public bool isInit()
    {
        return this.init;
    }

    private void addStats()
    {
        stats.addBestVisibility(getBestVisibility());
        stats.addBestFitness(getBestFitness());
        stats.addBestLocations(getBestLocations());
    }

    public Individual getBest()
    {
        return individuals[0];
    }

    public float getAverageFitness()
    {
        float totalFitness = 0;

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
        return getBest().visibleSamples / (float) visiableSamples;
    }

    public float getBestFitness()
    {
        return getBest().fitness;
    }

    public int getBestLocations()
    {
        return getBest().computedGridPoints.Length;
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

        if (generation == 1000)
        {
            JsonExporter.export(stats);
        }
    }

    private void Sort()
    {
        individuals.Sort((a, b) => b.fitness.CompareTo(a.fitness));
    }
}

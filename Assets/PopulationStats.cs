using System.Collections.Generic;

class PopulationStats
{
    public int populationSize;
    public int crossoverRate;
    public int mutationRate;
    public int elists;

    public List<float> bestVisibility;
    public List<float> bestFitness;
    public List<int> bestLocations;
    public List<float> averageLocations;
    public List<float> averageFitness;
    public List<float> standardDeviation;

    public PopulationStats()
    {
        bestVisibility = new List<float>();
        bestFitness = new List<float>();
        bestLocations = new List<int>();
        averageLocations = new List<float>();
        standardDeviation = new List<float>();
        averageFitness = new List<float>();
    }

    public void addBestVisibility(float f)
    {
        bestVisibility.Add(f);
    }

    public void addBestFitness(float f)
    {
        bestFitness.Add(f);
    }

    public void addBestLocations(int i)
    {
        bestLocations.Add(i);
    }

    public void addAverageLocations(float f)
    {
        averageLocations.Add(f);
    }

    public void addAverageFitness(float f)
    {
        averageFitness.Add(f);
    }

    public void addStandardDeviation(float f)
    {
        standardDeviation.Add(f);
    }
}
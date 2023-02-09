using System.Collections.Generic;

class PopulationStats
{
    public List<float> bestVisibility;
    public List<float> bestFitness;
    public List<int> bestLocations;
    public List<int> averageLocations;

    public PopulationStats()
    {
        bestVisibility = new List<float>();
        bestFitness = new List<float>();
        bestLocations = new List<int>();
        averageLocations = new List<int>();
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

    public void addAverageLocations(int i)
    {
        averageLocations.Add(i);
    }
}
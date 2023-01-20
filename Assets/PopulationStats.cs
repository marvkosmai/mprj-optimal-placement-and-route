using System.Collections.Generic;

class PopulationStats
{
    public List<float> bestVisibility;

    public PopulationStats()
    {
        bestVisibility = new List<float>();
    }

    public void addBestVisibility(float v)
    {
        bestVisibility.Add(v);
    }
}
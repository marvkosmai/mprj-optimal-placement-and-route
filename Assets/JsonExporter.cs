using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
class JsonExporter
{
    public static void export(PopulationStats stats)
    {
        string json = JsonConvert.SerializeObject(stats);

        File.WriteAllText(
            $"p{stats.populationSize}_" +
            $"c{stats.crossoverRate}_" +
            $"m{stats.mutationRate}_" +
            $"e{stats.elists}_" +
            $"g{stats.bestFitness.Count}_" +
            $"{DateTime.Now.Ticks}" +
            $".json", json);
    }
}
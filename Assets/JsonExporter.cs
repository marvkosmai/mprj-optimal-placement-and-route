using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
class JsonExporter
{
    public static void export(PopulationStats stats)
    {
        string json = JsonConvert.SerializeObject(stats);

        File.WriteAllText("stats.json", json);
    }
}
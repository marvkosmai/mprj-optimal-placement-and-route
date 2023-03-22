using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Route
{
    public float fitness;

    private float[,] map;
    public int[] route;

    public Route(float[,] map, int[] route)
    {
        this.map = map;
        this.route = route;
    }

    public float CalcFitness()
    {
        fitness = 0f;

        int count = 1;

        int start = route[0];
        for (int i = 1; i < route.Length; i++)
        {
            int end = route[i];
            float distance = map[start, end];

            if (0 > distance)
            {
                count++;
                distance = 100.0f;
            }

            fitness += distance;
            start = end;
        }
        fitness += map[start, route[0]];

        fitness *= count;

        return fitness;
    }
}
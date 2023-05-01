using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Route
{
    public float fitness;
    public int errors;

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

        float maxLength = 0;
        for (int i = 0; i < route.Length; i++)
        {
            for (int j = 0; j < route.Length; j++)
            {
                if (maxLength < map[i, j])
                {
                    maxLength = map[i, j];
                }
            }
        }

        int start = route[0];
        for (int i = 1; i < route.Length; i++)
        {
            int end = route[i];
            float distance = map[start, end];

            if (0 > distance)
            {
                count++;
                distance += maxLength;
            }

            fitness += distance;
            start = end;
        }
        if (map[start, route[0]] > 0)
        {
            fitness += map[start, route[0]];
        }
        else
        {
            count++;
            fitness += maxLength;
        }

        errors = count;
        

        fitness *= count;

        return fitness;
    }
}
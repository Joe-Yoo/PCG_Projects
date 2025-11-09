using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;

public class MeshCoordinates
{
    public static List<(int, int, int)> Body()
    {
        List<(int, int, int)> result = new List<(int, int, int)>();

        for (int x = 0; x < 12; x++)
            for (int y = 0; y < 7; y++)
                for (int z = 0; z < 7; z++)
                    if (!(y == 6 && (z == 0 || z == 6)))
                        result.Add((x, y, z));

        for (int y = -1; y >= -2; y--)
        {
            result.Add((1, y, 1));
            result.Add((10, y, 5));
            result.Add((10, y, 1));
            result.Add((1, y, 5));
        }

        for (int x = -4; x < 1; x++)
            for (int y = 4; y < 9; y++)
                for (int z = 1; z < 6; z++)
                    if (!result.Contains((x,y,z)))
                        result.Add((x, y, z));

        return result;
    }

    public static List<(int, int, int)> Wings() {
        List<(int, int, int)> result = new List<(int, int, int)>();

        for (int i = 0; i <= 3; i++) {
            result.Add((-1,i,0));
        }

        for (int y = 0; y < 6; y++)
            for (int x = 0; x <= y; x++) 
                if (y - x <= 4)
                    result.Add((x,y,0));
            
        return result;
    }

    public static List<(int, int, int)> Nostril_1() {
        List<(int, int, int)> result = new List<(int, int, int)>();

        for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                for (int z = 0; z < 3; z++)
                    result.Add((x,y,z));
            
        return result;
    } 

    public static List<(int, int, int)> Eyes() {
        List<(int, int, int)> result = new List<(int, int, int)>();

        result.Add((0,0,0));
        result.Add((0,0,2));
            
        return result;
    }

    public static List<(int, int, int)> Tail_1() {
        List<(int, int, int)> result = new List<(int, int, int)>();

        result.Add((0, 3, 0));
        result.Add((1, 3, 0));
        result.Add((1, 2, 0));
        result.Add((2, 2, 0));
        result.Add((2, 1, 0));
        result.Add((2, 0, 0));
            
        return result;
    }

    public static List<(int, int, int)> Tail_2() {
        List<(int, int, int)> result = new List<(int, int, int)>();

        result.Add((0, 3, 0));
        result.Add((1, 3, 0));
        result.Add((1, 2, 0));
        result.Add((2, 2, 0));
        result.Add((2, 1, 0));
        result.Add((2, 0, 0));
            
        return result;
    }

    public static List<(int, int, int)> Single() {
        return new List<(int, int, int)>() { (0, 0, 0) };
    }
}
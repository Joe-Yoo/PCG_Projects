using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;

public class MeshCoordinates
{
    public static List<(int, int, int)> test_0 = new List<(int, int, int)>
    {
        (0,0,0), (1,0,0), (0,0,1), (1,0,1), (0,0,2), (1,0,2),
        (0,1,0), (1,1,0), (0,1,1), (1,1,1), (0,1,2), (1,1,2),
    };

    public static List<(int, int, int)> Test_1()
    {
        List<(int, int, int)> result = new List<(int, int, int)>();

        for (int x = 0; x < 12; x++)
            for (int y = 0; y < 7; y++)
                for (int z = 0; z < 7; z++)
                {
                    if (!(y == 6 && (z == 0 || z == 6)))
                        result.Add((x, y, z));
                }

        result.Add((12, 4, 3));
        result.Add((13, 4, 3));
        result.Add((13, 3, 3));
        result.Add((13, 2, 3));

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
                {
                    if (!result.Contains((x,y,z)))
                        result.Add((x, y, z));
                }


        return result;
    }
}
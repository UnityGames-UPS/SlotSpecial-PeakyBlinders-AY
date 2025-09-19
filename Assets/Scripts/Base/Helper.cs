using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class Helper : MonoBehaviour
{

    internal static List<string> FlattenSymbolsToEmit(List<List<string>> symbolsToEmit)
    {
        List<string> flattenedList = new List<string>();

        // Flatten the list
        foreach (var innerList in symbolsToEmit)
        {
            flattenedList.AddRange(innerList);
        }

        return flattenedList;
    }

    internal static List<string> RemoveDuplicates(List<List<string>> inputList)
    {
        if (inputList == null) return null;

        HashSet<string> uniqueStrings = new HashSet<string>(FlattenSymbolsToEmit(inputList));
        return new List<string>(uniqueStrings);
    }

    internal static List<List<int>> ConvertFrozenIndicesToCoord(List<List<double>> frozenindices)
    {

        List<List<int>> coords = new List<List<int>>();

        for (int i = 0; i < frozenindices.Count; i++)
        {
            List<int> coord = new List<int>
        {
            (int)frozenindices[i][0],
            (int)frozenindices[i][1]
        };
            coords.Add(coord);

        }

        Debug.Log(JsonConvert.SerializeObject(coords));
        return coords;
    }
    internal static List<string> Convert2dToLinearMatrix(List<List<int>> matrix)
    {
        List<string> finalMatrix = new List<string>();
        for (int j = 0; j < matrix[0].Count; j++)
        {
            string n = "";
            for (int i = 0; i < matrix.Count; i++)
            {
                n += matrix[i][j];
            }
            finalMatrix.Add(n);

        }

        return finalMatrix;
    }
    internal static List<string> Convert2dToLinearMatrix(List<List<string>> matrix)
    {
        List<string> finalMatrix = new List<string>();
        for (int j = 0; j < matrix[0].Count; j++)
        {
            string n = "";
            for (int i = 0; i < matrix.Count; i++)
            {
                n += matrix[i][j];
            }
            finalMatrix.Add(n);
        }

        return finalMatrix;
    }

    internal static List<List<int>> ConvertStringMatrixToIntMatrix(List<List<string>> matrix)
    {
        List<List<int>> intMatrix = new List<List<int>>();

        foreach (var row in matrix)
        {
            List<int> intRow = new List<int>();
            foreach (var str in row)
            {
                if (int.TryParse(str, out int value))
                    intRow.Add(value);

            }
            intMatrix.Add(intRow);
        }

        return intMatrix;
    }


    public static List<string> GetAllSymbolToEmit(Payload payload, List<List<int>> allLines)
    {

        List<List<int>> linesToEmit = new List<List<int>>();
        List<string> symbolsToEmit = new List<string>();
        foreach (var lineWin in payload.lineWins)
        {

            string symbolPositions = "";
            int lineIndex = lineWin.lineIndex;

            foreach (int col in lineWin.positions)
            {

                // row comes from master payline definition
                int row = allLines[lineIndex][col];
                symbolPositions = col.ToString() + "," + row.ToString();

                symbolsToEmit.Add(symbolPositions);
            }

        }

        return symbolsToEmit;
    }


    public static List<List<int>> GetAllFrozenIndexex(List<FrozenIndex> frozenIndices)
    {
        List<List<int>> linesToEmit = new List<List<int>>();
        for (int i = 0; i < frozenIndices.Count; i++)
        {
            linesToEmit.Add(frozenIndices[i].position);
        }
        return linesToEmit;
    }

}

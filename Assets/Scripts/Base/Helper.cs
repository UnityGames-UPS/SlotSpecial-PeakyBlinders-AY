using System.Collections.Generic;
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

    internal static List<string> Convert2dToLinearMatrix(List<List<int>> matrix)
    {
        List<string> finalMatrix=new List<string>();
        for (int j = 0; j < matrix[0].Count; j++)
        {
            string n="";
            for (int i = 0; i < matrix.Count; i++)
            {
                    n+=matrix[i][j];
            }
            finalMatrix.Add(n);
            
        }

        return finalMatrix;
    }

}

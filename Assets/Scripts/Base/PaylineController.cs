using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class PaylineController : MonoBehaviour
{
    [SerializeField] private int x_Distance;
    [SerializeField] private int y_Distance;
    [SerializeField] private Transform LineContainer;
    [SerializeField] private UILineRenderer Line_Prefab;
    [SerializeField] private Vector2 InitialLinePosition = new Vector2(-315, 100);

    [SerializeField]internal List<List<int>> paylines=new List<List<int>>();
 
    internal void GeneratePayline(int lineId)
    {
        UILineRenderer line = Instantiate(Line_Prefab, LineContainer);

        line.transform.localPosition = new Vector2(InitialLinePosition.x, InitialLinePosition.y);
        var pointlist = new List<Vector2>();
        Vector2 points = new Vector2(0, 0);
        for (int j = 0; j < paylines[lineId].Count; j++)
        {
            points.x = j * x_Distance;
            points.y = paylines[lineId][j] * -y_Distance;
            pointlist.Add(points);
        }
        line.Points = pointlist.ToArray();

    }



    //delete all lines
    internal void ResetLines()
    {
        foreach (Transform child in LineContainer)
        {
            Destroy(child.gameObject);
        }
    }





}
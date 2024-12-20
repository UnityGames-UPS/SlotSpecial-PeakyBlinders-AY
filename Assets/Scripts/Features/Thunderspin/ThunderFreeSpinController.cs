using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
public class ThunderFreeSpinController : MonoBehaviour
{
    [SerializeField] List<rows> SpinMatrix;
    [SerializeField] GameObject thunderSpinLayer;
    [SerializeField] Sprite coinBase;
    [SerializeField] Sprite noValue;

    float totalDelay;

    internal Action<int, double> updateUI;
    Coroutine Spin;

    internal Func<Action, Action, bool, bool, float, float, IEnumerator> SpinRoutine;

    [SerializeField] GameObject horizontalbar;
    internal IEnumerator StartFP(List<List<double>> froxenIndeces, int count)
    {

        horizontalbar.SetActive(true);
        Initiate(froxenIndeces);
        int i = 0;
        while (i < count)
        {
            Spin = StartCoroutine(SpinRoutine(null, CloseIcon, false, true, 0, totalDelay));
            yield return Spin;
            ResetIcon(false);
            i++;
            updateUI(count - i, 0);
            if (SocketModel.resultGameData.thunderSpinAdded)
            {
                if (Spin != null)
                    StopCoroutine(Spin);
                count = SocketModel.resultGameData.thunderSpinCount;
                i = 0;
                updateUI(count, 0);
            }
            if (SocketModel.resultGameData.isGrandPrize)
                break;

        }
        thunderSpinLayer.SetActive(false);
        ResetIcon(true);
        horizontalbar.SetActive(false);


        yield return null;
    }


    void Initiate(List<List<double>> froxenIndeces)
    {

        for (int i = 0; i < froxenIndeces.Count; i++)
        {
            SpinMatrix[(int)froxenIndeces[i][0]].row[(int)froxenIndeces[i][1]].image.sprite = coinBase;
            SpinMatrix[(int)froxenIndeces[i][0]].row[(int)froxenIndeces[i][1]].coinText.text = froxenIndeces[i][2].ToString();
            SpinMatrix[(int)froxenIndeces[i][0]].row[(int)froxenIndeces[i][1]].coinText.gameObject.SetActive(true);
            SpinMatrix[(int)froxenIndeces[i][0]].row[(int)froxenIndeces[i][1]].image.transform.localPosition *= 0;
            SpinMatrix[(int)froxenIndeces[i][0]].row[(int)froxenIndeces[i][1]].hasValue = true;

        }
        thunderSpinLayer.SetActive(true);


    }

    void CloseIcon()
    {

        StartCoroutine(closeSlotIcon());
    }
    IEnumerator closeSlotIcon()
    {

        for (int i = 0; i < SocketModel.resultGameData.frozenIndices.Count; i++)
        {
            if (!SpinMatrix[(int)SocketModel.resultGameData.frozenIndices[i][0]].row[(int)SocketModel.resultGameData.frozenIndices[i][1]].hasValue)
            {
                SpinMatrix[(int)SocketModel.resultGameData.frozenIndices[i][0]].row[(int)SocketModel.resultGameData.frozenIndices[i][1]].image.sprite = coinBase;
                SpinMatrix[(int)SocketModel.resultGameData.frozenIndices[i][0]].row[(int)SocketModel.resultGameData.frozenIndices[i][1]].hasValue = true;
                SpinMatrix[(int)SocketModel.resultGameData.frozenIndices[i][0]].row[(int)SocketModel.resultGameData.frozenIndices[i][1]].coinText.gameObject.SetActive(true);
                SpinMatrix[(int)SocketModel.resultGameData.frozenIndices[i][0]].row[(int)SocketModel.resultGameData.frozenIndices[i][1]].coinText.text = SocketModel.resultGameData.frozenIndices[i][2].ToString();

                SpinMatrix[(int)SocketModel.resultGameData.frozenIndices[i][0]].row[(int)SocketModel.resultGameData.frozenIndices[i][1]].image.transform.DOLocalMoveY(0, 0.15f).SetEase(Ease.OutBounce);
                yield return new WaitForSeconds(0.15f);
                totalDelay++;
            }
        }

        for (int i = 0; i < SpinMatrix.Count; i++)
        {
            for (int j = 0; j < SpinMatrix[i].row.Count; j++)
            {

                if (!SpinMatrix[i].row[j].hasValue)
                {
                    // SpinMatrix[i].row[j].image.sprite = noValue;
                    SpinMatrix[i].row[j].image.transform.DOLocalMoveY(0, 0.15f).SetEase(Ease.OutBounce);
                    yield return new WaitForSeconds(0.15f);
                    totalDelay++;
                }

            }
        }
        totalDelay *= 0.15f;

    }

    void ResetIcon(bool hard)
    {
        for (int i = 0; i < SpinMatrix.Count; i++)
        {
            for (int j = 0; j < SpinMatrix[i].row.Count; j++)
            {
                if (hard)
                {
                    SpinMatrix[i].row[j].image.sprite = noValue;
                    SpinMatrix[i].row[j].image.transform.localPosition = new Vector2(0, 225);
                }
                else if (!SpinMatrix[i].row[j].hasValue)
                {
                    SpinMatrix[i].row[j].image.sprite = noValue;
                    SpinMatrix[i].row[j].image.transform.localPosition = new Vector2(0, 225);
                }

            }
        }
    }


    [Serializable]
    public class rows
    {
        public List<ThunderSpinIconView> row = new List<ThunderSpinIconView>();
    }


}

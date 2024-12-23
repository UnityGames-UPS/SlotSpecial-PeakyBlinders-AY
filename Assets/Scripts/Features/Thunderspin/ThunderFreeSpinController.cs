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
    [SerializeField] internal List<Sprite> imageRef;
    float totalDelay;

    internal Action<int, double> UpdateUI;
    Coroutine Spin;

    internal Func<Action, Action, bool, bool, float, float, IEnumerator> SpinRoutine;

    [SerializeField] GameObject horizontalbar;

    internal Action<int, GameObject> FreeSpinPopUP;
    internal Action<GameObject> FreeSpinPopUpClose;

    [SerializeField] GameObject thunderSpinBg;
    internal IEnumerator StartFP(List<List<double>> froxenIndeces, int count)
    {
        FreeSpinPopUP?.Invoke(count, thunderSpinBg);
        yield return new WaitForSeconds(2);
        FreeSpinPopUpClose?.Invoke(thunderSpinBg);
        horizontalbar.SetActive(true);
        Initiate(froxenIndeces);
        while (count > 0)
        {
            count--;
            UpdateUI?.Invoke(count, -1);

            Spin = StartCoroutine(SpinRoutine(null, CloseIcon, false, true, 0, totalDelay));
            yield return Spin;
            ResetIcon(false);
            if (SocketModel.resultGameData.thunderSpinAdded)
            {
                if (Spin != null)
                    StopCoroutine(Spin);
                int prevFreeSpin = count;
                count = SocketModel.resultGameData.thunderSpinCount;
                int freeSpinAdded = count - prevFreeSpin;
                FreeSpinPopUP?.Invoke(freeSpinAdded, null);
                UpdateUI(count, 0);
                yield return new WaitForSeconds(1.5f);
                FreeSpinPopUpClose?.Invoke(null);
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
            Debug.Log(froxenIndeces[i].Count);
            SpinMatrix[(int)froxenIndeces[i][0]].row[(int)froxenIndeces[i][1]].image.sprite = imageRef[(int)froxenIndeces[i][3]];
            if((int)froxenIndeces[i][3]==13){
            SpinMatrix[(int)froxenIndeces[i][0]].row[(int)froxenIndeces[i][1]].coinText.text = froxenIndeces[i][2].ToString();
            SpinMatrix[(int)froxenIndeces[i][0]].row[(int)froxenIndeces[i][1]].coinText.gameObject.SetActive(true);

            }
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
                SpinMatrix[(int)SocketModel.resultGameData.frozenIndices[i][0]].row[(int)SocketModel.resultGameData.frozenIndices[i][1]].image.sprite = imageRef[(int)SocketModel.resultGameData.frozenIndices[i][3]];
                SpinMatrix[(int)SocketModel.resultGameData.frozenIndices[i][0]].row[(int)SocketModel.resultGameData.frozenIndices[i][1]].hasValue = true;
                if((int)SocketModel.resultGameData.frozenIndices[i][3]==13){

                SpinMatrix[(int)SocketModel.resultGameData.frozenIndices[i][0]].row[(int)SocketModel.resultGameData.frozenIndices[i][1]].coinText.text = SocketModel.resultGameData.frozenIndices[i][2].ToString();
                SpinMatrix[(int)SocketModel.resultGameData.frozenIndices[i][0]].row[(int)SocketModel.resultGameData.frozenIndices[i][1]].coinText.gameObject.SetActive(true);
                }

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
                    SpinMatrix[i].row[j].hasValue=false;
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

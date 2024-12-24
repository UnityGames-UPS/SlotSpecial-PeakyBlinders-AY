using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
public class ThunderFreeSpinController : MonoBehaviour
{
    [SerializeField] List<rows> SpinMatrix;
    [SerializeField] GameObject thunderSpinLayer;
    [SerializeField] Sprite noValue;
    [SerializeField] internal List<Sprite> imageRef;

    [SerializeField] List<Sprite> animSprite;
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
            yield return new WaitForSeconds(0.5f);
            
        }
        thunderSpinLayer.SetActive(false);
        ResetIcon(true);
        horizontalbar.SetActive(false);


        yield return null;
    }


    void Initiate(List<List<double>> froxenIndeces)
    {
        ThunderSpinIconView icon = null;

        for (int i = 0; i < froxenIndeces.Count; i++)
        {
            Debug.Log(froxenIndeces[i].Count);
            icon = SpinMatrix[(int)froxenIndeces[i][0]].row[(int)froxenIndeces[i][1]];
            icon.image.sprite = imageRef[(int)froxenIndeces[i][3]];

            if ((int)froxenIndeces[i][3] == 13)
            {
                icon.coinText.text = froxenIndeces[i][2].ToString();
                icon.coinText.gameObject.SetActive(true);
            }
            
            icon.image.transform.localPosition *= 0;
            icon.hasValue = true;
            icon.StartAnim(animSprite);


        }
        thunderSpinLayer.SetActive(true);


    }

    void CloseIcon()
    {

        StartCoroutine(closeSlotIcon());
    }
    IEnumerator closeSlotIcon()
    {
        ThunderSpinIconView icon = null;

        for (int i = 0; i < SocketModel.resultGameData.frozenIndices.Count; i++)
        {
            icon = SpinMatrix[(int)SocketModel.resultGameData.frozenIndices[i][0]].row[(int)SocketModel.resultGameData.frozenIndices[i][1]];

            if (!icon.hasValue)
            {
                icon.image.sprite = imageRef[(int)SocketModel.resultGameData.frozenIndices[i][3]];
                icon.hasValue = true;

                if ((int)SocketModel.resultGameData.frozenIndices[i][3] == 13)
                {

                    icon.coinText.text = SocketModel.resultGameData.frozenIndices[i][2].ToString();
                    icon.coinText.gameObject.SetActive(true);
                }

                icon.image.transform.DOLocalMoveY(0, 0.15f).SetEase(Ease.OutBounce);
                yield return new WaitForSeconds(0.15f);
                icon.StartAnim(animSprite);
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
                    SpinMatrix[i].row[j].hasValue = false;
                    SpinMatrix[i].row[j].StopAnim();

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

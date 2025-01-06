using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Newtonsoft.Json;
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
    internal Action FreeSpinPopUPOverlay;

    internal float totalSlotClosingDelay=0;
    [SerializeField] GameObject thunderSpinBg;
    internal Action< List<List<int>>,List<List<double>> > populateOriginalMatrix;
    internal IEnumerator StartFP(List<List<double>> froxenIndeces, int count, List<List<int>> ResultReel)
    {
        FreeSpinPopUPOverlay?.Invoke();
        yield return new WaitWhile(()=>UIManager.freeSpinOverLayOpen);
        FreeSpinPopUP?.Invoke(count, thunderSpinBg);
        yield return new WaitForSeconds(1.8f);
        FreeSpinPopUpClose?.Invoke(thunderSpinBg);
        horizontalbar.SetActive(true);
        Initiate(froxenIndeces);
        // ResetIcon(false);

        while (count > 0)
        {
            count--;
            UpdateUI?.Invoke(count, -1);
            // ResetIcon(false);
            Spin = StartCoroutine(SpinRoutine(()=>ResetIcon(false), CloseIcon, false, true, 0, totalDelay));
            yield return Spin;
            totalDelay=0;
            if (SocketModel.resultGameData.thunderSpinAdded)
            {
                if (Spin != null)
                    StopCoroutine(Spin);
                int prevFreeSpin = count;
                count = SocketModel.resultGameData.thunderSpinCount;
                int freeSpinAdded = count - prevFreeSpin;
                UpdateUI?.Invoke(count, -1);
                FreeSpinPopUP?.Invoke(freeSpinAdded, null);
                yield return new WaitForSeconds(1.5f);
                FreeSpinPopUpClose?.Invoke(null);
            }
            if (SocketModel.resultGameData.isGrandPrize)
                break;
                
            if(SocketModel.playerData.currentWining>0)
            yield return new WaitForSeconds(3f);
            else
            yield return new WaitForSeconds(1f);
            
        }
        Debug.Log("teststdsad"+JsonConvert.SerializeObject(froxenIndeces));
        Debug.Log("sdsdsd"+JsonConvert.SerializeObject(ResultReel));
        if(populateOriginalMatrix!=null)
        Debug.Log("sddsdsd");

        populateOriginalMatrix(ResultReel,froxenIndeces);
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
                icon.coinText.text = froxenIndeces[i][2].ToString()+"X";
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
                 DOTween.Kill(icon.image.transform);
                icon.image.transform.localPosition=new Vector2(0,225);
                icon.image.sprite = imageRef[(int)SocketModel.resultGameData.frozenIndices[i][3]];
                icon.hasValue = true;

                if ((int)SocketModel.resultGameData.frozenIndices[i][3] == 13)
                {
                    icon.coinText.text = SocketModel.resultGameData.frozenIndices[i][2].ToString()+"X";
                    icon.coinText.gameObject.SetActive(true);
                }else{
                    icon.coinText.gameObject.SetActive(false);
                }

                icon.image.transform.DOLocalMoveY(0, 0.15f).SetEase(Ease.OutBounce);
                icon.StartAnim(animSprite);
                totalDelay+=0.15f;
                yield return new WaitForSeconds(0.15f);
            }

        }

        for (int i = 0; i < SpinMatrix.Count; i++)
        {
            for (int j = 0; j < SpinMatrix[i].row.Count; j++)
            {

                if (!SpinMatrix[i].row[j].hasValue)
                {
                    // SpinMatrix[i].row[j].image.sprite = noValue
                    DOTween.Kill(SpinMatrix[i].row[j].image.transform);
                     SpinMatrix[i].row[j].image.transform.localPosition=new Vector2(0,225);
                    SpinMatrix[i].row[j].image.transform.DOLocalMoveY(0, 0.15f).SetEase(Ease.OutBounce);
                    totalDelay+=0.15f;
                   
                }

            }
             yield return new WaitForSeconds(0.15f);
        }

        Debug.Log("total dealy"+totalDelay);
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
                    DOTween.Kill(SpinMatrix[i].row[j].image.transform);
                    SpinMatrix[i].row[j].coinText.gameObject.SetActive(false);
                    SpinMatrix[i].row[j].StopAnim();

                }
                else if (!SpinMatrix[i].row[j].hasValue)
                {
                    SpinMatrix[i].row[j].image.sprite = noValue;
                    if( SpinMatrix[i].row[j].coinText.gameObject.activeSelf)
                    SpinMatrix[i].row[j].coinText.gameObject.SetActive(false);
                    SpinMatrix[i].row[j].image.transform.DOLocalMoveY(-225,0.15f);
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

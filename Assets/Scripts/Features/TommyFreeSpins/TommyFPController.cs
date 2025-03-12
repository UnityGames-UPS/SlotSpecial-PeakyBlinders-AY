using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Newtonsoft.Json;
public class TommyFPController : MonoBehaviour
{
    [SerializeField] Transform colossalSlot;
    [SerializeField] ImageAnimation colossalIcon;

    [SerializeField] float tweenHeight;

    [SerializeField] float initialPos;

    Tweener alltweens;

    internal Func<Action, Action, bool, bool, float, float, IEnumerator> SpinRoutine;

    Coroutine spin;

    internal Action<int, double> UpdateUI;

    internal Action<int, GameObject> FreeSpinPopUP;
    internal Action FreeSpinPopUPOverlay;
    internal Action<GameObject> FreeSpinPopUpClose;
    [SerializeField] GameObject tommySpinBg;

    [SerializeField] internal ThunderFreeSpinController thunderFP;
    [SerializeField] internal Sprite[] iconImages;

    [Header("Animation Sprites")]
    [SerializeField] private List<Sprite> ID_0;
    [SerializeField] private List<Sprite> ID_1;
    [SerializeField] private List<Sprite> ID_2;
    [SerializeField] private List<Sprite> ID_3;
    [SerializeField] private List<Sprite> ID_4;
    [SerializeField] private List<Sprite> ID_5;
    [SerializeField] private List<Sprite> ID_6;
    [SerializeField] private List<Sprite> ID_7;
    [SerializeField] private List<Sprite> ID_8;
    [SerializeField] private List<Sprite> ID_9;
    [SerializeField] private List<Sprite> ID_10;
    int colIndex = -1;
        int id = -1;

    internal IEnumerator StartFP(int count)
    {
        //play by the order of peaky blider animation
        FreeSpinPopUPOverlay?.Invoke();
        yield return new WaitWhile(()=>!UIManager.freeSpinOverLayOpen);
        // show user freespin count and specific BG
        FreeSpinPopUP?.Invoke(count, tommySpinBg);
        yield return new WaitForSeconds(1.8f);
        FreeSpinPopUpClose?.Invoke(tommySpinBg);
        colossalSlot.parent.gameObject.SetActive(true);
        // colossalSlot.gameObject.SetActive(true);

        while (count > 0)
        {
            count--;
                // update only count
            UpdateUI?.Invoke(count, -1);
            //start spinning the slot with specific action after the spin and before the spin
            yield return spin = StartCoroutine(SpinRoutine(StartColossalSpin, StopTweening, false, false, 0.5f, 0.5f));
                // update only balance
            UpdateUI?.Invoke(-1, SocketModel.playerData.currentWining);
            colIndex=-1;
            id=-1;
            // for new free spin added
            if (SocketModel.resultGameData.freeSpinAdded)
            {
                if (spin != null)
                    StopCoroutine(spin);
                int prevFreeSpin = count;
                count = SocketModel.resultGameData.freeSpinCount;
                int freeSpinAdded = count - prevFreeSpin;
                // update only count
                UpdateUI?.Invoke(count, -1);
                FreeSpinPopUP?.Invoke(freeSpinAdded, null);
                yield return new WaitForSeconds(1.5f);
                FreeSpinPopUpClose?.Invoke(null);

            }
            // for thunder spin added
            if (SocketModel.resultGameData.thunderSpinCount > 0)
            {
                if (spin != null)
                    StopCoroutine(spin);
                colossalSlot.parent.gameObject.SetActive(false);
                yield return thunderFP.StartFP(
                froxenIndeces: SocketModel.resultGameData.frozenIndices,
                count: SocketModel.resultGameData.thunderSpinCount,
                ResultReel: SocketModel.resultGameData.ResultReel
                );
                colossalSlot.parent.gameObject.SetActive(true);

            }
            if (SocketModel.playerData.currentWining > 0)
                yield return new WaitForSeconds(2.5f);
            else
                yield return new WaitForSeconds(1f);

        }

        colossalSlot.parent.gameObject.SetActive(false);
        colossalSlot.gameObject.SetActive(false);

    }
    private void StartColossalSpin()
    {
        //find the id of the colossal icon and from which column it is coming
        colIndex = FindColIndex(SocketModel.resultGameData.ResultReel);
        if (colIndex < 0 || id<0)
            return;
            colossalIcon.StopAnimation();

        //spin the colossal icon
        colossalSlot.transform.localPosition = new Vector3(-270 + colIndex * 270, colossalSlot.transform.localPosition.y);
        colossalIcon.transform.localScale = new Vector2(0, 0);
        colossalSlot.gameObject.SetActive(true);
        colossalIcon.transform.DOScale(1, 0.35f).OnComplete(() =>
        {

            Tweener tweener = colossalSlot.DOLocalMoveY(-tweenHeight, 1f).SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);
            alltweens = tweener;
        });


    }

    private void StopTweening()
    {
        if (colIndex < 0 || id<0)
            return;
        //populate the colossal icon with the specific icon
            PopulateSpriteNAnim(id);
        alltweens?.Pause();
        colossalSlot.localPosition = new Vector2(colossalSlot.localPosition.x, initialPos + 680);
        alltweens = colossalSlot.DOLocalMoveY(initialPos, 0.2f).OnComplete(() =>
        {
            colossalIcon.StartAnimation();
            // StartCoroutine(StartDissolvAnim());
        });

    }


    private int FindColIndex(List<List<int>> resultReel)
    {
        //converts restult reel to linear matrix of string coulmn wise 
        List<string> convertedmatrix = Helper.Convert2dToLinearMatrix(resultReel);
        int index=-1;
        id=-1;
        //check for consicutives 3 same columns and ite start index and its symbol
        for (int i = 0; i < convertedmatrix.Count; i++)
        {
            if (i + 2 < convertedmatrix.Count)
            {

                if (convertedmatrix[i] == convertedmatrix[i + 1] && convertedmatrix[i + 1] == convertedmatrix[i + 2])
                {
                    index = i;
                    id = SocketModel.resultGameData.ResultReel[0][i];
                    break;
                }

            }
        }


        Debug.Log(colIndex);
        return index;


    }

    void PopulateSpriteNAnim(int id)
    {
        colossalIcon.textureArray.Clear();
        switch (id)
        {
            case 0:
                colossalIcon.rendererDelegate.sprite = ID_0[0];
                colossalIcon.textureArray.AddRange(ID_0);
                break;
            case 1:
                colossalIcon.rendererDelegate.sprite = ID_1[0];
                colossalIcon.textureArray.AddRange(ID_1);
                break;
            case 2:
                colossalIcon.rendererDelegate.sprite = ID_2[0];
                colossalIcon.textureArray.AddRange(ID_2);
                break;
            case 3:
                colossalIcon.rendererDelegate.sprite = ID_3[0];
                colossalIcon.textureArray.AddRange(ID_3);
                break;
            case 4:
                colossalIcon.rendererDelegate.sprite = ID_4[0];
                colossalIcon.textureArray.AddRange(ID_4);
                break;
            case 5:
                colossalIcon.rendererDelegate.sprite = ID_5[0];
                colossalIcon.textureArray.AddRange(ID_5);
                break;
            case 6:
                colossalIcon.rendererDelegate.sprite = ID_6[0];
                colossalIcon.textureArray.AddRange(ID_6);
                break;
            case 7:
                colossalIcon.rendererDelegate.sprite = ID_7[0];
                colossalIcon.textureArray.AddRange(ID_7);
                break;
            case 8:
                colossalIcon.rendererDelegate.sprite = ID_8[0];
                colossalIcon.textureArray.AddRange(ID_8);
                break;
            case int n when n > 8 && n < 13:
                colossalIcon.rendererDelegate.sprite = ID_9[0];
                colossalIcon.textureArray.AddRange(ID_9);
                break;
            case int n when n >= 13:
                colossalIcon.rendererDelegate.sprite = ID_10[0];
                colossalIcon.textureArray.AddRange(ID_10);
                break;

        }
    }

}

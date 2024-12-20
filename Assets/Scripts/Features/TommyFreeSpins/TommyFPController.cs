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
    [SerializeField] Image colossalIcon;

    [SerializeField] float tweenHeight;

    [SerializeField] float initialPos;

    [SerializeField] Material dissolveMaterial;
    Tweener alltweens;

    [SerializeField] internal List<Sprite> spriteRef;
    internal Func<Action, Action, bool, bool, float, float, IEnumerator> SpinRoutine;

    Coroutine spin;

    internal Action<int, double> UpdateUI;
    internal IEnumerator StartFP(int count)
    {
        colossalSlot.parent.gameObject.SetActive(true);

        while (count > 0)
        {
            count--;
            UpdateUI?.Invoke(count,-1);
            yield return spin = StartCoroutine(SpinRoutine(StartColossalSpin, StopTweening, false, false, 0.5f, 0.5f));
            UpdateUI?.Invoke(-1,SocketModel.playerData.currentWining);
            if (SocketModel.resultGameData.freeSpinAdded)
            {
                if (spin != null)
                    StopCoroutine(spin);
                int prevFreeSpin = count;
                count = SocketModel.resultGameData.freeSpinCount;
                int freeSpinAdded=count - prevFreeSpin;
                
                yield return new WaitForSeconds(1.5f);

            }

        }

        colossalSlot.parent.gameObject.SetActive(false);

    }
    private void StartColossalSpin()
    {
        colossalSlot.transform.localPosition = new Vector3(-270 + FindColIndex() * 270, colossalSlot.transform.localPosition.y);
        colossalIcon.transform.localScale = new Vector2(0, 0);
        colossalSlot.gameObject.SetActive(true);
        colossalIcon.transform.DOScale(1, 0.35f).OnComplete(() =>
        {

            Tweener tweener = colossalSlot.DOLocalMoveY(-tweenHeight, 1.2f).SetLoops(-1, LoopType.Restart).SetDelay(0).SetEase(Ease.Linear);
            alltweens = tweener;
        });


    }

    private void StopTweening()
    {
        alltweens?.Pause();
        colossalSlot.localPosition = new Vector2(colossalSlot.localPosition.x, initialPos + 680);
        alltweens = colossalSlot.DOLocalMoveY(initialPos, 0.2f).OnComplete(() =>
        {
            StartCoroutine(StartDissolvAnim());
        });

    }

    IEnumerator StartDissolvAnim()
    {
        colossalIcon.material = dissolveMaterial;

        float currentThreshold = 0;
        while (currentThreshold <= 1)
        {
            colossalIcon.material.SetFloat("_Threshold", currentThreshold);
            currentThreshold += Time.deltaTime;
            yield return null;
        }
        colossalSlot.gameObject.SetActive(false);
        colossalIcon.material.SetFloat("_Threshold", 0);
        colossalIcon.material = null;
    }

    private int FindColIndex()
    {
        int index = -1;

        List<string> convertedmatrix = Helper.Convert2dToLinearMatrix(SocketModel.resultGameData.ResultReel);

        for (int i = 0; i < convertedmatrix.Count; i++)
        {
            if (i + 2 < convertedmatrix.Count)
            {

                if (convertedmatrix[i] == convertedmatrix[i + 1] && convertedmatrix[i + 1] == convertedmatrix[i + 2])

                    index = i;
                colossalIcon.sprite = spriteRef[SocketModel.resultGameData.ResultReel[0][i]];
            }
        }

        return index;


    }
}

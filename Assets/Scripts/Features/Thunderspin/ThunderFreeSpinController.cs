using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Newtonsoft.Json;
using TMPro;
public class ThunderFreeSpinController : MonoBehaviour
{
    [SerializeField] private SocketController socketController;
    [SerializeField] List<rows> SpinMatrix;
    [SerializeField] GameObject thunderSpinLayer;
    [SerializeField] Sprite noValue;
    [SerializeField] internal List<Sprite> imageRef;
    [SerializeField] List<Sprite> animSprite;
    float totalDelay = 14 * 0.15f;
    Coroutine Spin;
    internal Func<Action, Action, bool, bool, float, float, IEnumerator> SpinRoutine;

    [SerializeField] GameObject horizontalbar;

    internal Action<int, GameObject> FreeSpinPopUP;
    internal Action<GameObject> FreeSpinPopUpClose;
    internal Action FreeSpinPopUPOverlay;

    internal float totalSlotClosingDelay = 0;
    [SerializeField] GameObject thunderSpinBg;
    internal Action<List<List<int>>, List<FrozenIndex>> populateOriginalMatrix;
    [SerializeField] private TMP_Text totalWinning_value;
    internal Action StopAllWinAnimation;
    private double currentTotalWinning;
    internal Action PlayStopSpinAudio;
    internal Action<double> thunderWinPopup;

    [SerializeField] private GameObject thundeSpinInfo_object;
    [SerializeField] private TMP_Text thundeSpinInfo_Text;
    internal IEnumerator StartFP(List<FrozenIndex> froxenIndeces, int count, List<List<int>> ResultReel)
    {
        GameManager.thunderFreeSpins = true;
        FreeSpinPopUPOverlay?.Invoke();
        thundeSpinInfo_object.SetActive(true);
        yield return new WaitWhile(() => UIManager.freeSpinOverLayOpen);
        FreeSpinPopUP?.Invoke(count, thunderSpinBg);
        yield return new WaitForSeconds(1.8f);
        FreeSpinPopUpClose?.Invoke(thunderSpinBg);
        horizontalbar.SetActive(true);
        Initiate(froxenIndeces);
        StopAllWinAnimation?.Invoke();
        // ResetIcon(false, true);
        yield return new WaitForSeconds(0.3f);
        totalDelay = froxenIndeces.Count * 0.15f;

        while (count > 0)
        {
            count--;
            thundeSpinInfo_Text.text = count.ToString();
            // UpdateInfo?.Invoke(count);
            // ResetIcon(false);
            Spin = StartCoroutine(SpinRoutine(() => ResetIcon(false), CloseIcon, false, true, 0.2f, totalDelay));
            yield return Spin;
            totalDelay = socketController.resultGameData.features.bonus.frozenIndices.Count * 0.15f;
            if (socketController.resultGameData.features.bonus.thunderSpinAdded)
            {
                if (Spin != null)
                    StopCoroutine(Spin);
                int prevFreeSpin = count;
                count = socketController.resultGameData.features.bonus.thunderSpinCount;
                int freeSpinAdded = count - prevFreeSpin;
                thundeSpinInfo_Text.text = count.ToString();
                FreeSpinPopUP?.Invoke(freeSpinAdded, null);
                yield return new WaitForSeconds(1.5f);
                FreeSpinPopUpClose?.Invoke(null);
            }
            if (socketController.resultGameData.features.isGrandPrize)                      //ashu
                break;

            if (socketController.resultGameData.payload.currentWinning > 0)
                yield return new WaitForSeconds(3f);
            else
                yield return new WaitForSeconds(1f);

        }

        for (int i = 0; i < SpinMatrix.Count; i++)
        {
            for (int j = 0; j < SpinMatrix[i].row.Count; j++)
            {

                if (SpinMatrix[i].row[j].hasValue)
                {
                    DOTween.Kill(SpinMatrix[i].row[j].coinText.transform);
                    if (!SpinMatrix[i].row[j].coinText.gameObject.activeSelf)
                        SpinMatrix[i].row[j].coinText.gameObject.SetActive(true);

                    SpinMatrix[i].row[j].coinText.transform.SetParent(totalWinning_value.transform.parent);
                    SpinMatrix[i].row[j].coinText.transform.DOLocalMove(Vector3.zero, 0.6f).SetEase(Ease.Linear);
                    yield return new WaitForSeconds(0.6f);
                    if (SpinMatrix[i].row[j].coinText.gameObject.activeSelf)
                        SpinMatrix[i].row[j].coinText.gameObject.SetActive(false);

                    currentTotalWinning += SpinMatrix[i].row[j].value;
                    totalWinning_value.text = currentTotalWinning.ToString() + "X";
                }

            }
        }
        // yield return new WaitForSeconds(1f);
        GameManager.winanimRunning = true;
        thunderWinPopup?.Invoke(socketController.resultGameData.payload.currentWinning);
        yield return new WaitUntil(() => !GameManager.winanimRunning);
        for (int i = 0; i < SpinMatrix.Count; i++)
        {
            for (int j = 0; j < SpinMatrix[i].row.Count; j++)
            {

                DOTween.Kill(SpinMatrix[i].row[j].coinText.transform);
                SpinMatrix[i].row[j].coinText.transform.SetParent(SpinMatrix[i].row[j].image.transform.parent);
                SpinMatrix[i].row[j].coinText.transform.localPosition = Vector3.zero;

            }
        }
        StopAllWinAnimation?.Invoke();
        populateOriginalMatrix?.Invoke(ResultReel, froxenIndeces);
        thundeSpinInfo_object.SetActive(false);
        thundeSpinInfo_Text.text = "0";

        thunderSpinLayer.SetActive(false);
        ResetIcon(true);
        horizontalbar.SetActive(false);

        currentTotalWinning = 0;
        totalWinning_value.text = currentTotalWinning.ToString("f3");
        GameManager.thunderFreeSpins = false;

        yield return null;
    }


    void Initiate(List<FrozenIndex> froxenIndeces)
    {
        ThunderSpinIconView icon = null;

        for (int i = 0; i < froxenIndeces.Count; i++)
        {
            // Debug.Log(froxenIndeces[i].Count);
            icon = SpinMatrix[(int)froxenIndeces[i].position[1]].row[(int)froxenIndeces[i].position[0]];
            int symbolIndex = int.Parse(socketController.resultGameData.features.bonus.frozenIndices[i].symbol);
            icon.image.sprite = imageRef[symbolIndex];
            icon.value = froxenIndeces[i].coinsvalue;
            icon.coinText.text = froxenIndeces[i].coinsvalue.ToString() + "X";
            if (symbolIndex == 13)
            {
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

        for (int i = 0; i < socketController.resultGameData.features.bonus.frozenIndices.Count; i++)
        {

            icon = SpinMatrix[(int)socketController.resultGameData.features.bonus.frozenIndices[i].position[1]].row[(int)socketController.resultGameData.features.bonus.frozenIndices[i].position[0]];

            if (!icon.hasValue)
            {
                DOTween.Kill(icon.image.transform);
                icon.image.transform.localPosition = new Vector2(0, 225);
                int symbolIndex = int.Parse(socketController.resultGameData.features.bonus.frozenIndices[i].symbol);
                icon.image.sprite = imageRef[symbolIndex];
                icon.hasValue = true;
                icon.value = socketController.resultGameData.features.bonus.frozenIndices[i].coinsvalue;
                icon.coinText.text = icon.value.ToString() + "X";
                if (symbolIndex == 13)
                {
                    icon.coinText.gameObject.SetActive(true);
                }
                else
                {
                    icon.coinText.gameObject.SetActive(false);
                }
                PlayStopSpinAudio?.Invoke();
                icon.image.transform.DOLocalMoveY(0, 0.15f).SetEase(Ease.Linear);
                icon.StartAnim(animSprite);
                totalDelay += 0.15f;
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
                    SpinMatrix[i].row[j].image.transform.localPosition = new Vector2(0, 225);
                    SpinMatrix[i].row[j].image.transform.DOLocalMoveY(0, 0.15f).SetEase(Ease.Linear);

                }

            }
            PlayStopSpinAudio?.Invoke();

            // totalDelay += 0.15f;
            // yield return new WaitForSeconds(0.15f);
        }

        Debug.Log("total dealy" + totalDelay);
    }

    void ResetIcon(bool hard, bool atonce = false)
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
                    if (SpinMatrix[i].row[j].coinText.gameObject.activeSelf)
                        SpinMatrix[i].row[j].coinText.gameObject.SetActive(false);
                    SpinMatrix[i].row[j].image.transform.DOLocalMoveY(-225, 0.15f);
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

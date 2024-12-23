using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json;
public class ArthurFreeSpinController : MonoBehaviour
{
    [SerializeField] internal List<SlotImage> slotMatrix = new List<SlotImage>();
    [SerializeField] float iconHeight = 225;
    [SerializeField] float minClearDuration = 0.2f;
    [SerializeField] GameObject psudoReel;
    [SerializeField] Sprite[] ID_0;
    [SerializeField] Sprite[] ID_1;
    [SerializeField] Sprite[] ID_2;
    [SerializeField] Sprite[] ID_3;

    [SerializeField] Transform blade;

    [SerializeField] internal List<Sprite> iconref;
    internal Action<List<List<int>>, List<List<double>>> populateOriginalMatrix;

    internal Func<Action, Action, bool, bool, float, float, IEnumerator> SpinRoutine;

    Coroutine spin;
    internal Action<int, double> UpdateUI;
    internal Action<int, GameObject> FreeSpinPopUP;
    internal Action<GameObject> FreeSpinPopUpClose;
    [SerializeField] GameObject arthurSpinBg;


    [SerializeField] internal ThunderFreeSpinController thunderFP;

    internal IEnumerator StartFP(GameObject originalReel, int count, bool initiate = true)
    {
        FreeSpinPopUP?.Invoke(count, arthurSpinBg);
        yield return new WaitForSeconds(2);
        FreeSpinPopUpClose?.Invoke(arthurSpinBg);

        if (initiate)
            yield return InitiateFreeSpins(originalReel);


        while (count > 0)
        {
            count--;
            UpdateUI?.Invoke(count, -1);
            yield return spin = StartCoroutine(SpinRoutine(null, null, false, false, 0, 0));
            UpdateUI?.Invoke(-1, SocketModel.playerData.currentWining);
            if (SocketModel.resultGameData.freeSpinAdded)
            {
                if (spin != null)
                    StopCoroutine(spin);
                int prevFreeSpin = count;
                count = SocketModel.resultGameData.freeSpinCount;
                int freeSpinAdded = count - prevFreeSpin;

                FreeSpinPopUP?.Invoke(freeSpinAdded, null);
                UpdateUI?.Invoke(count, -1);

                yield return new WaitForSeconds(1.5f);
                FreeSpinPopUpClose?.Invoke(null);

            }

            if (SocketModel.resultGameData.thunderSpinCount > 0)
            {
                if (spin != null)
                    StopCoroutine(spin);

                yield return thunderFP.StartFP(
                froxenIndeces: SocketModel.resultGameData.frozenIndices,
                count: SocketModel.resultGameData.thunderSpinCount);

            }

        }

        originalReel.SetActive(true);
        psudoReel.SetActive(true);

    }

    IEnumerator InitiateFreeSpins(GameObject originalReel)
    {

        for (int i = 0; i < slotMatrix.Count; i++)
        {
            for (int j = 0; j < slotMatrix[i].slotImages.Count; j++)
            {
                PopulateAnimation(slotMatrix[i].slotImages[j].activeanimation, slotMatrix[i].slotImages[j].id);
            }
        }

        psudoReel.SetActive(true);
        originalReel.SetActive(false);
        yield return PlayCutAnimation();
        yield return RearrangeMatrix();
        originalReel.SetActive(true);
        yield return null;
    }



    internal IEnumerator RearrangeMatrix()
    {


        for (int j = slotMatrix[0].slotImages.Count - 1; j >= 0; j--)
        {
            for (int i = 0; i < slotMatrix.Count; i++)
            {
                if (slotMatrix[i].slotImages[j].id < 4)
                {
                    slotMatrix[i].slotImages[j].id = -1;
                    slotMatrix[i].slotImages[j].transform.DOLocalMoveY(-5 * iconHeight, (minClearDuration + UnityEngine.Random.Range(0, 0.2f)) * (2 - j + 1)).SetEase(Ease.Linear);
                }
            }

        }
        yield return new WaitForSeconds(1.4f);

        for (int i = 0; i < slotMatrix.Count; i++)
        {
            var negativeOnes = slotMatrix[i].slotImages.Where(x => x.id == -1).ToList();

            var otherValues = slotMatrix[i].slotImages.Where(x => x.id != -1).ToList();

            if (negativeOnes.Count == 0)
                continue;

            foreach (var item in otherValues)
            {
                negativeOnes.Add(item);
            }

            slotMatrix[i].slotImages.Clear();
            slotMatrix[i].slotImages.AddRange(negativeOnes);


            for (int j = 0; j < slotMatrix[i].slotImages.Count; j++)
            {
                if (slotMatrix[i].slotImages[j].id == -1)
                {
                    slotMatrix[i].slotImages[j].transform.localPosition = new Vector3(slotMatrix[i].slotImages[j].transform.localPosition.x, 5 * iconHeight, slotMatrix[i].slotImages[j].transform.localPosition.z);

                    int randomIndex = UnityEngine.Random.Range(4, 8);

                    slotMatrix[i].slotImages[j].iconImage.sprite = iconref[randomIndex];
                    slotMatrix[i].slotImages[j].id = randomIndex;
                }
                slotMatrix[i].slotImages[j].transform.DOLocalMoveY((2 - j) * iconHeight + iconHeight / 2, minClearDuration).SetEase(Ease.InOutQuad);
                Debug.Log("pos" + ((2 - j) * iconHeight - iconHeight));
            }

        }



        List<List<int>> finalResult = new List<List<int>>();

        for (int i = 0; i < slotMatrix[0].slotImages.Count; i++)
        {
            List<int> temp = new List<int>();

            for (int j = 0; j < slotMatrix.Count; j++)
            {
                temp.Add(slotMatrix[j].slotImages[i].id);
            }
            finalResult.Add(temp);
        }
        populateOriginalMatrix?.Invoke(finalResult, null);
        yield return new WaitForSeconds(minClearDuration + 0.5f);
        psudoReel.SetActive(false);

    }

    IEnumerator PlayCutAnimation()
    {
        blade.gameObject.SetActive(true);
        for (int j = 0; j < slotMatrix[0].slotImages.Count; j++)
        {
            blade.DOLocalMoveX(1600 * (j % 2 == 0 ? 1 : -1), 0.35f).OnComplete(() => blade.transform.localPosition += new Vector3(0, -225, 0));
            for (int i = 0; i < slotMatrix.Count; i++)
            {
                if (slotMatrix[i].slotImages[j].id < 4)
                {
                    slotMatrix[i].slotImages[j].activeanimation.StartAnimation();

                }
            }

            yield return new WaitForSeconds(0.5f);

        }
        yield return new WaitForSeconds(0.5f);
        blade.gameObject.SetActive(false);
        blade.transform.localPosition = new Vector3(-1600, 360, 0);

    }
    void PopulateAnimation(ImageAnimation imageAnimation, int id)
    {


        imageAnimation.textureArray.Clear();
        switch (id)
        {
            case 0:
                imageAnimation.textureArray.AddRange(ID_0);
                break;
            case 1:
                imageAnimation.textureArray.AddRange(ID_1);
                break;
            case 2:
                imageAnimation.textureArray.AddRange(ID_2);
                break;
            case 3:
                imageAnimation.textureArray.AddRange(ID_3);
                break;
        }

    }

}

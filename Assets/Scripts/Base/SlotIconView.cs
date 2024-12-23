using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotIconView : MonoBehaviour
{
    [Header("required fields")]
    [SerializeField] internal int pos;
    [SerializeField] internal int id = -1;
    [SerializeField] internal Image iconImage;
    [SerializeField] internal Image iconBorderImage;
    [SerializeField] internal GameObject iconBg;
    [SerializeField] internal ImageAnimation activeanimation;
    [SerializeField] private int siblingIndex;
    [SerializeField] private Material ignoreMask;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Transform ownParent;
    private void Start()
    {
        siblingIndex = transform.GetSiblingIndex();
        ownParent = transform.parent;
    }


    internal void SetIcon(Sprite image, int id, int pos)
    {
        iconImage.sprite = image;
        this.id = id;
        this.pos = pos;

        if (id > 8 && id < 13)
        {
            activeanimation.rendererDelegate = iconImage;
        }
        else
        {
            activeanimation.rendererDelegate = iconBorderImage;
        }
    }

    internal void SetCoin(double value)
    {
        text.text = value.ToString()+"X";
        text.gameObject.SetActive(true);
    }

    internal void ClearCoin()
    {
        if (text.gameObject.activeSelf)
        {

            text.gameObject.SetActive(false);
            text.text = "";
        }
    }
    internal void Ignoremask()
    {
        iconImage.material = ignoreMask;
    }

    internal void RespectMask()
    {
        // transform.SetSiblingIndex(siblingIndex);
        iconImage.material = null;

    }
    internal void StartAnim(List<Sprite> animSprite)
    {
        activeanimation.textureArray.Clear();
        activeanimation.textureArray.AddRange(animSprite);
        activeanimation.AnimationSpeed = animSprite.Count / 2 + 0.5f;
        if(id <=8 || id >= 13)
        iconBorderImage.gameObject.SetActive(true);
        activeanimation.StartAnimation();
    }

    internal void StopAnim()
    {

        activeanimation.StopAnimation();
        activeanimation.textureArray.Clear();
        // activeanimation.textureArray.Add(firstSprite);

    }

    internal void SetParent(Transform paylineSymbolAnimPanel)
    {
        if (id < 9 || id > 12)
        {
            iconBorderImage.gameObject.SetActive(true);
            // iconBg.SetActive(true);
        }
        transform.SetParent(paylineSymbolAnimPanel);
        transform.SetAsLastSibling();
    }
    internal void ResetParent()
    {
        // if (id < 9 && id>12){

        DeActivateIconBorder();
        // iconBg.SetActive(false);
        // }

        transform.SetParent(ownParent);
        transform.SetSiblingIndex(siblingIndex);

    }

    internal void DeActivateIconBorder()
    {
        if (iconBorderImage.gameObject.activeSelf)
            iconBorderImage.gameObject.SetActive(false);
    }

}

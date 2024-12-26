using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ButtonAnimation : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    [SerializeField]  Button selfButton;
    public void OnPointerDown(PointerEventData eventData)
    {
        if(selfButton.interactable)
        transform.localScale*=0.9f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(transform.localScale.x<1)
        transform.localScale= Vector3.one;

    }

    // Start is called before the first frame update


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

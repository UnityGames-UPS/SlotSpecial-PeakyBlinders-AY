using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThunderSpinIconView : MonoBehaviour
{
    [SerializeField] internal Image image;
    [SerializeField] internal TMP_Text coinText;
    
    [SerializeField] internal ImageAnimation anim;
    internal bool hasValue;


    internal void StartAnim(List<Sprite> animSPrite){

        anim.textureArray.Clear();
        anim.textureArray.AddRange(animSPrite);

        anim.gameObject.SetActive(true);
        

    }

    internal void StopAnim(){
        anim.gameObject.SetActive(false);
        anim.textureArray.Clear();

    }
    
}

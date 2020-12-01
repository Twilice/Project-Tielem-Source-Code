using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class UI_WeaponShop_ItemLevel : MonoBehaviour
{
    public int level = 1;
    public GameObject activeImage;
    [ReadOnlyInInspector]
    public UI_WeaponShop_Item item;

    public void SetUIActive()
    {
        activeImage.SetActive(true);
    }


    public void SetUIDisabled()
    {
        activeImage.SetActive(false);
    }
}
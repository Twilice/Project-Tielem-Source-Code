using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Obsolete]
public class UI_Select_PrimaryWeapon : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public static List<UI_Select_PrimaryWeapon> ui_weapons = new List<UI_Select_PrimaryWeapon>();
    public PrimaryWeapon primaryWeapon;
    public Button selectButton;
    public Button downgradeButton;
    public Button upgradeButton;
    public Text levelText;
    
    void Start()
    {
        if(ui_weapons.Count == 0)
            transform.parent.GetComponentsInChildren(ui_weapons);
        
        if(ui_weapons[0] == this)
        {
            ui_weapons[0].SelectWeapon();
        }
    }

    public void DisableOutOfBounds()
    {
        downgradeButton.interactable = true;
        upgradeButton.interactable = true;

        if (GameCoordinator.instance.data.primaryWeaponLevel == 1)
        {
            downgradeButton.interactable = false;
        }
        if(GameCoordinator.instance.data.primaryWeaponLevel == GameCoordinator.instance.data.equippedPrimaryWeapon.weaponSlots.Count)
        {
            upgradeButton.interactable = false;
        }
    }

    public void SelectWeapon()
    {
        GameCoordinator.instance.RegisterPrimaryWeapon(primaryWeapon);
        foreach(var weapon in ui_weapons)
        {
            if (weapon != this)
            {
                weapon.selectButton.animator.ResetTrigger("Selected");
                weapon.selectButton.animator.SetTrigger("Deselect");
                weapon.levelText.gameObject.SetActive(false);
                weapon.downgradeButton.interactable = false;
                weapon.upgradeButton.interactable = false;
            }
            else
            {
                weapon.selectButton.animator.ResetTrigger("Deselect");
                weapon.selectButton.animator.SetTrigger("Selected");
                weapon.levelText.text = GameCoordinator.instance.data.primaryWeaponLevel.ToString();
                weapon.levelText.gameObject.SetActive(true);
                weapon.downgradeButton.interactable = true;
                weapon.upgradeButton.interactable = true;
            }
        }
        DisableOutOfBounds();
    }

    public void UpgradeWeapon()
    {
        GameCoordinator.instance.data.equippedPrimaryWeapon.UpgradeLevel();
        DisableOutOfBounds();
        levelText.text = GameCoordinator.instance.data.primaryWeaponLevel.ToString();
    }

    public void DowngradeWeapon()
    {
        GameCoordinator.instance.data.equippedPrimaryWeapon.DowngradeLevel();
        DisableOutOfBounds();
        levelText.text = GameCoordinator.instance.data.primaryWeaponLevel.ToString();
    }


    // todo :: for possible hover effects on whole weapon (much much later)
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
    }
}

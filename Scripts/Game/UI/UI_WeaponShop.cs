using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_WeaponShop : MonoBehaviour
{
    public Transform shopItemContainer;
    [ReadOnlyInInspector]
    public List<UI_WeaponShop_Item> weapons = new List<UI_WeaponShop_Item>(); // make tuple with sale info
    [ReadOnlyInInspector]
    public UI_WeaponShop_Item equippedWeapon;

    void Start()
    {
        if (shopItemContainer == null)
            Debug.LogError($"{nameof(shopItemContainer)} missing in {nameof(UI_WeaponShop)} - {name}");



        foreach (var item in GameCoordinator.instance.data.data_PrimaryWeapons.primaryWeaponUIPrefabs)
        {
            Instantiate(item, shopItemContainer);
        }

        // todo :: we want to randomize the weapons depending on some conditions and sales etc. Each new level should have a semi-randomed selection of weapons with randomised cost (sales) Equipped weapon should always be existing.
        GetComponentsInChildren(weapons);
        
        equippedWeapon = weapons.Find(weapon => weapon.primaryWeapon == GameCoordinator.instance.data.equippedPrimaryWeaponPrefab);

        foreach (var weapon in weapons)
        {
            weapon.weaponShop = this;
        }

        GameCoordinator.instance.weaponShop = this;
    }

    public void Init()
    {
        foreach (var weapon in weapons)
        {
            weapon.Init();
        }
    }


    public void EquipdWeapon(UI_WeaponShop_Item itemToKeepActive)
    {
        equippedWeapon = itemToKeepActive;
        foreach (var weapon in weapons)
        {
            if(weapon != itemToKeepActive)
                weapon.DisableAllLevels();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemToBuy"></param>
    /// <param name="itemLevel">Null equals the items current level.</param>
    /// <returns></returns>
    public int GetItemValue(UI_WeaponShop_Item itemToBuy, int? itemLevel = null)
    {
        int value = 0;
        int level = itemLevel ?? itemToBuy.level;

        for (int i = 0; i < level; i++)
        {
            value += itemToBuy.cost[i]; // todo :: apply sale to weapon
        }

        return value;
    }

}

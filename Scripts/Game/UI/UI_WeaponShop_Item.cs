using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UI_WeaponShop_Item : MonoBehaviour
{
    public PrimaryWeapon primaryWeapon;
    [ReadOnlyInInspector]
    public UI_WeaponShop weaponShop;
    public List<int> cost = new List<int>{1000, 200, 300, 400, 500}; // note: only plan for exactly 5 levels for now.
    public bool selected = false;
    public int level = 0;
    public List<UI_WeaponShop_ItemLevel> itemLevels = new List<UI_WeaponShop_ItemLevel>();


#if UNITY_EDITOR
    public SceneAsset previewSceneAsset;
#endif
    public string previewSceneName;
    [ReadOnlyInInspector]
    public PortraitRenderer portraitRenderer;
    [ReadOnlyInInspector]
    public UI_PreviewPortrait_Area previewPortraitArea;


    // note :: items are dynamically created, awake instead of start so references will be set before instantiate is complete.
    void Awake()
    {
        GetComponentsInChildren(itemLevels);

        if (itemLevels.Count != cost.Count)
        {
            for(int i = itemLevels.Count-1; i >=0; i--)
            {
                if (i < cost.Count)
                {
                    itemLevels[i].item = this;
                }
                else
                {
                    itemLevels[i].gameObject.SetActive(false);
                    itemLevels.RemoveAt(i);
                }
            }
        }
        else
        {
            foreach (var itemLevel in itemLevels)
            {
                itemLevel.item = this;
            }
        }
    }

    public void Init()
    {
        if (GameCoordinator.instance.data.equippedPrimaryWeaponPrefab == primaryWeapon)
        {
            level = GameCoordinator.instance.data.primaryWeaponLevel;
            SetActivePreview(level);

            foreach (var itemLevel in itemLevels)
            {
                if (itemLevel.level <= level)
                    itemLevel.SetUIActive();
                else itemLevel.SetUIDisabled();
            }
        }
        else
        {
            foreach (var itemLevel in itemLevels)
            {
                itemLevel.SetUIDisabled();
            }
        }
    }

    public void DisableAllLevels()
    {
        foreach (var itemLevel in itemLevels)
        {
            itemLevel.SetUIDisabled();
        }
    }

    public void SetActivePreview(int level)
    {
        portraitRenderer.previewSceneRoot.GetComponentInChildren<PrimaryWeapon>().ApplyLevel(level);
        if (previewPortraitArea.currentPortraitRenderer != portraitRenderer)
        {
            previewPortraitArea.SetPortraitRenderer(portraitRenderer);
        }
    }

    /// <summary>
    /// Called from ActionButton
    /// </summary>
    /// <param name="itemClicked"></param>
    public void ShopLevelClicked(GameObject itemClicked)
    {
        var clickedItemLevel = itemClicked.GetComponent<UI_WeaponShop_ItemLevel>();
        SetActivePreview(clickedItemLevel.level);
        
        // note: we want to include the shop potential sales in the price.
        int previousItemValue = weaponShop.GetItemValue(weaponShop.equippedWeapon);
        int newItemCost = weaponShop.GetItemValue(clickedItemLevel.item, clickedItemLevel.level);

        if (previousItemValue + GameCoordinator.instance.currencyValue < newItemCost)
        {
            GameCoordinator.PlayOneShot(GameCoordinator.instance.data.errorSound);
        }
        else
        {
            GameCoordinator.instance.currencyValue += previousItemValue - newItemCost;
            level = clickedItemLevel.level;
            weaponShop.EquipdWeapon(this);
            GameCoordinator.instance.data.equippedPrimaryWeaponPrefab = primaryWeapon;
            GameCoordinator.instance.data.primaryWeaponLevel = level;

            foreach (var itemLevel in itemLevels)
            {
                if (itemLevel.level <= clickedItemLevel.level)
                    itemLevel.SetUIActive();
                else
                    itemLevel.SetUIDisabled();
            }
        }
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(UI_WeaponShop_Item), editorForChildClasses: true)]
public class UI_WeaponShop_ItemEditor : Editor
{

    SerializedObject previousValue = null;
    SerializedProperty sceneAsset;
    private void OnEnable()
    {
        sceneAsset = serializedObject.FindProperty(nameof(UI_WeaponShop_Item.previewSceneAsset));
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (previousValue != sceneAsset.serializedObject)
        {
            UI_WeaponShop_Item itemLevel = (UI_WeaponShop_Item)target;
            var newValue = sceneAsset.objectReferenceValue as SceneAsset;
            if (newValue != null)
                itemLevel.previewSceneName = newValue.name;
        }
    }
}
#endif
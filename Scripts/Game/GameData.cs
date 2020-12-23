using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameData", order = 1)]
public class GameData : ScriptableObject
{
    public Data_PrimaryWeapons data_PrimaryWeapons;
    public PrimaryWeapon equippedPrimaryWeaponPrefab;
    [ReadOnlyInInspector]
    public PrimaryWeapon equippedPrimaryWeapon;
    public int primaryWeaponLevel = 1;
    public AudioClip errorSound;
    public GameObject fadeCanvas;
}

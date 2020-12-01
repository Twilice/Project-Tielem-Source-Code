using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data_PrimaryWeapons", menuName = "ScriptableObjects/Data_PrimaryWeapons", order = 1)]
public class Data_PrimaryWeapons : ScriptableObject
{
    // todo :: add droptable?
    public List<GameObject> primaryWeaponUIPrefabs;
}

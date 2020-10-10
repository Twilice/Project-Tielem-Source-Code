using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameData", order = 1)]
public class GameData : ScriptableObject
{
    public PrimaryWeapon selectedPrimaryWeapon;
    public int primaryWeaponLevel = 1;
    public Weapon selectedWeapon1;
    public Weapon selectedWeapon2;
}

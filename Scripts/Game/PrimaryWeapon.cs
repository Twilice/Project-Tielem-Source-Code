using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvocationFlow;
using System.Linq;

[System.Serializable]
public class WeaponSlot
{
    public GameObject Anchor;
    [HideInInspector]
    public List<Weapon> weapons;

    public void Init()
    {
        Anchor.GetComponentsInChildren<Weapon>(weapons);
    }

    public void Fire()
    {
        foreach (var weapon in weapons)
        {
            weapon.Fire();
        }
    }

    public void TryFire()
    {
        foreach(var weapon in weapons)
        {
            weapon.TryFire();
        }
    }
}

// upgradeable weapon with paths - should hold + activate different weapons depending on level.
public class PrimaryWeapon : GameEntity
{

    // debug :: for lazy debug in inspector only
    public float energy;

    public int level = 0;
    [ReadOnlyInInspector]
    public WeaponSlot activeWeaponSlot;
    public List<WeaponSlot> weaponSlots;
    public int maxLevel = 0;

    public void Awake()
    {
        foreach (var slot in weaponSlots)
        {
            slot.Init();
            maxLevel++;
            slot.Anchor.SetActive(false);
            foreach (var weapon in slot.weapons)
            {
                weapon.owner = this;
            }
        }
        if (level != 0)
            ApplyLevel(level);
    }

    public void ApplyLevel(int level)
    {
        this.level = level;
        activeWeaponSlot = weaponSlots[level - 1];
        foreach (var slot in weaponSlots)
        {
            slot.Anchor.SetActive(false);
        }
        activeWeaponSlot.Anchor.SetActive(true);
    }

    public void UpgradeLevel()
    {
        level++;
        if (level > maxLevel)
        {
            level = maxLevel;
        }
    }

    public void DowngradeLevel()
    {
        level--;
        if (level < 1)
        {
            level = 1;
        }
    }

    private bool isFiring = false;
    private float primaryWeaponWindup = 0;
    public void OnStartFiring(GameEntity entity)
    {
        isFiring = true;
        if (primaryWeaponWindup > 0) return;

        if (activeWeaponSlot.weapons.Any(weapon => weapon.cooldown > 0))
        {
            primaryWeaponWindup = activeWeaponSlot.weapons.Aggregate(0f, (highestCooldown, weapon) => highestCooldown > weapon.baseCooldown + weapon.windDown ? highestCooldown : weapon.baseCooldown + weapon.windDown);
            this.InvokeWhileThen(() => primaryWeaponWindup -= Time.deltaTime, () => primaryWeaponWindup > 0, () =>
             {
                 if (isFiring)
                 {
                     foreach (var weapon in weaponSlots.SelectMany(slot => slot.weapons))
                     {
                         weapon.OnStartFiring(entity);
                     }
                 }
             });
        }
        else
        {
            foreach (var weapon in weaponSlots.SelectMany(slot => slot.weapons))
            {
                weapon.OnStartFiring(entity);
            }
        }
    }
    public void OnStopFiring(GameEntity entity, OnStoppedFiringEventArgs args)
    {
        isFiring = false;
        foreach (var weapon in weaponSlots.SelectMany(slot => slot.weapons))
        {
            weapon.OnStopFiring(entity, args);
        }
    }


    public void TryFire()
    {
        activeWeaponSlot.TryFire();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvocationFlow;

[System.Serializable]
public class WeaponSlot
{
    public GameObject Anchor;
    public int UpgradeCost;
    public List<Weapon> weapons;

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
    public float baseDamage = 2.0f;
    public float baseCooldown = 0.5f;
    public float baseWindUp = 0;
    public float baseChargeUp = 0;

    [ReadOnlyInInspector]
    public float cooldown = 0;
    [ReadOnlyInInspector]
    public float windUp = 0;
    [ReadOnlyInInspector]
    public float chargeUp = 0;

    // prefabs
    public Bullet bullet;
    public GameObject muzzleEffect;

    public float bulletLifetime = 5f;
    public EnergyConsumer energyConsumer;

    // debug :: for lazy debug in inspector only
    public float energy;

    public int level = 1;
    [ReadOnlyInInspector]
    public WeaponSlot activeWeaponSlot;
    public List<WeaponSlot> weaponSlots;

    public bool CanFire => windUp >= baseWindUp && cooldown <= 0 && chargeUp >= baseChargeUp;

    public void Awake()
    {
        foreach (var slot in weaponSlots)
        {
            slot.Anchor.SetActive(false);
            foreach (var weapon in slot.weapons)
            {
                weapon.owner = this;
            }
        }

        if (energyConsumer.need != 0)
        {
            energyConsumer.Init(this);
            // debug :: lazy way to show energy value.
            this.InvokeWhile(() => energy = energyConsumer.energy, () => true);
        }
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

    public void OnStartFiring(GameEntity entity)
    {
        foreach (var weapon in activeWeaponSlot.weapons)
        {
            weapon.OnStartFiring(entity);
        }
    }
    public void OnStopFiring(GameEntity entity, OnStoppedFiringEventArgs args)
    {
        foreach(var weapon in activeWeaponSlot.weapons)
        {
            weapon.OnStopFiring(entity, args);
        }
    }


    public void TryFire()
    {
        activeWeaponSlot.TryFire();

        //if (CanFire)
        //{
        //    if (energyConsumer.IsUnused)
        //    {
        //        Fire();
        //    }
        //    else if (energyConsumer.HasEnoughEnergy())
        //    {
        //        energyConsumer.UseEnergy();
        //        Fire();
        //    }
        //}
    }

    public void Fire()
    {
        // reserved for special ability?
        cooldown += baseCooldown;

        // todo :: object pooling
        var firedBullet = Instantiate(bullet, transform.position, bullet.transform.rotation * transform.rotation);

        firedBullet.owner = this;
        firedBullet.lifeTime = bulletLifetime;
        firedBullet.damage = baseDamage;
        firedBullet.velocity = firedBullet.velocity.magnitude * transform.forward;

        if (muzzleEffect != null)
        {
            var spawnedMuzzle = Instantiate(muzzleEffect, transform.position, muzzleEffect.transform.rotation * transform.rotation);
            spawnedMuzzle.transform.parent = transform;
            this.InvokeDelayed(3f, () => Destroy(spawnedMuzzle));
        }
    }

    public void Update()
    {
        if(cooldown > 0)
            cooldown -= Time.deltaTime;
    }
}

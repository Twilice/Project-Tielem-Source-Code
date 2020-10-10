using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvocationFlow;
using System;

// standard weapons
public class Weapon : GameEntity
{
    public float baseDamage = 2.0f;
    public float baseCooldown = 0.5f;
    public float baseWindUp = 0;
    public float baseChargeUp = 0;
    public float baseWindDown = 0.25f;

    [ReadOnlyInInspector]
    public float cooldown = 0;
    [ReadOnlyInInspector]
    public float windUp = 0;
    [ReadOnlyInInspector]
    public float windDown = 0;
    [ReadOnlyInInspector]
    public float chargeUp = 0;

    // prefabs
    public Bullet bullet;
    public GameObject muzzleEffect;
    public GameObject chargeUpEffect;
    
    public float bulletLifetime = 5f;
    public EnergyConsumer energyConsumer;

    // debug :: for lazy debug in inspector only
    public float debugEnergyInspector;

    public bool CanFire => windUp >= baseWindUp && cooldown <= 0 && chargeUp >= baseChargeUp;

    public void Awake()
    {
        if (energyConsumer.need != 0)
        {
            energyConsumer.Init(this);
            // debug ::
            this.InvokeWhile(() => debugEnergyInspector = energyConsumer.energy, () => true);
        }
    }

    public bool woundUp = false;
    public bool isFiring = false;
    public void OnStartFiring(GameEntity entity)
    {
        isFiring = true;
        if (woundUp)
            windUp = baseWindUp;

        // todo :: add windupEffect?
    }

    public void OnStopFiring(GameEntity entity, OnStoppedFiringEventArgs args)
    {
        isFiring = false;
        windDown = baseWindDown;
    }

    public void TryFire()
    {
        if (CanFire)
        {
            if (energyConsumer.IsUnused)
            {
                Fire();
            }
            else if (energyConsumer.HasEnoughEnergy())
            {
                energyConsumer.UseEnergy();
                Fire();
            }
        }
    }

    public void Fire()
    {
        cooldown += baseCooldown;
        chargeUp = 0;

        // todo :: object pooling
        var firedBullet = Instantiate(bullet, transform.position, bullet.transform.rotation * transform.rotation);

        firedBullet.owner = this;
        firedBullet.lifeTime = bulletLifetime;
        firedBullet.damage = baseDamage;
        firedBullet.velocity = firedBullet.velocity.magnitude * transform.forward;
     
        if(muzzleEffect != null)
        {
            var spawnedMuzzle = Instantiate(muzzleEffect, transform.position, muzzleEffect.transform.rotation * transform.rotation);
            spawnedMuzzle.transform.parent = transform;
            this.InvokeDelayed(3f, () => Destroy(spawnedMuzzle));
        }
    }

    public void Update()
    {
        if (cooldown > 0)
            cooldown -= Time.deltaTime;
        else
            cooldown = 0;

        if((isFiring || windDown > 0) && cooldown <= 0)
        {
            windUp += Time.deltaTime;
            if (windUp >= baseWindUp)
            {
                windUp = baseWindUp;
                woundUp = true;
            }

            if(baseWindUp == 0 || woundUp)
            {
                if(baseChargeUp != 0 && chargeUp == 0)
                {
                    if (chargeUpEffect != null)
                    {
                        var spawnedChargeEffect = Instantiate(chargeUpEffect, transform.position, chargeUpEffect.transform.rotation * transform.rotation);
                        spawnedChargeEffect.transform.parent = transform;
                        this.InvokeDelayed(3f, () => Destroy(spawnedChargeEffect));
                    }
                }

                chargeUp += Time.deltaTime;
                if (chargeUp >= baseChargeUp)
                    chargeUp = baseChargeUp;
            }

        }

        if (isFiring == false)
        {
            windDown -= Time.deltaTime;
            if (windDown <= 0)
            {
                windDown = 0;

                windUp -= Time.deltaTime;
                if (windUp <= 0)
                {
                    windUp = 0;
                    woundUp = false;
                }
                chargeUp -= Time.deltaTime;
                if (chargeUp <= 0)
                    chargeUp = 0;
            }
            else
            {
                TryFire();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvocationFlow;
using System;
using UnityEngine.SceneManagement;

// standard weapons
public class Weapon : GameEntity
{
    public float baseDamage = 2.0f;
    public float baseCooldown = 0.5f;
    public float baseWindUp = 0;
    public float baseChargeUp = 0;
    [Tooltip("If set to -1 and weapon has not fired yet, will add enough winddown to fire once fired. Else if weapon has fired there is no winddown.")]
    public float baseWindDown = -1f;

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

    public bool CanFire => isFiring && woundUp && cooldown <= 0 && chargeUp >= baseChargeUp;

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
        hasFiredThisFiringCycle = false;
        isFiring = true;
        windDown = 0;
        // todo :: add windupEffect?
    }
    public void StopFiring()
    {
        isFiring = false;
        
        woundUp = baseWindUp == 0;
        windUp = 0;
        windDown = 0;
        chargeUp = 0;
    }

    public void OnStopFiring(GameEntity entity, OnStoppedFiringEventArgs args)
    {
        if (isFiring == false) return;

        if (baseWindDown == 0)
            StopFiring();

        else if (baseWindDown == -1)
        {
            if (hasFiredThisFiringCycle == false)
            {
                windDown = cooldown + baseWindUp - windUp + baseChargeUp - chargeUp + Time.deltaTime;
            }
            else StopFiring();
        }
        else
        {
            windDown = baseWindDown;
        }
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
    private bool hasFiredThisFiringCycle = false;
    public void Fire()
    {
        hasFiredThisFiringCycle = true;
        cooldown += baseCooldown;
        chargeUp -= baseChargeUp;

        // todo :: object pooling
        var firedBullet = Instantiate(bullet, transform.position, bullet.transform.rotation * transform.rotation);

        // note :: don't play audio if we are in mainmenu/shop
        if (GameCoordinator.instance.currentSceneType != GameCoordinator.SceneType.Level)
        {
            foreach (var audio in firedBullet.GetComponentsInChildren<AudioSource>())
            {
                audio.enabled = false;
            }
        }

        // note :: if we are in previewScene, custom logic
        if (firedBullet.gameObject.scene != gameObject.scene)
        {
            SceneManager.MoveGameObjectToScene(firedBullet.gameObject, gameObject.scene);
            firedBullet.transform.parent = gameObject.scene.GetRootGameObjects()[0].transform;
        }

        firedBullet.owner = this;
        firedBullet.lifeTime = bulletLifetime;
        firedBullet.damage = baseDamage;
        firedBullet.velocity = firedBullet.velocity.magnitude * transform.forward;

        GameObject spawnedMuzzle = null;
        if(muzzleEffect != null)
        {
            spawnedMuzzle = Instantiate(muzzleEffect, transform.position, muzzleEffect.transform.rotation * transform.rotation);

       
            if (GameCoordinator.instance.currentSceneType != GameCoordinator.SceneType.Level)
            {
                foreach (var audio in spawnedMuzzle.GetComponentsInChildren<AudioSource>())
                {
                    audio.enabled = false;
                }
            }
            spawnedMuzzle.transform.parent = transform;
            this.InvokeDelayed(3f, () => Destroy(spawnedMuzzle));
        }

        // note ::  workarround for portrait layer. We currently don't have a good way of automatically layering the bullets dynamically spawned in portraitScene.
        if (GameCoordinator.instance.currentSceneType == GameCoordinator.SceneType.Shop)
        {
            int portraitLayer = LayerMask.NameToLayer("PortraitRenderer");

            foreach (var t in firedBullet.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = portraitLayer;
            }

            if (spawnedMuzzle != null)
            {
                foreach (var t in spawnedMuzzle.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = portraitLayer;
                }
            }
        }
    }
    
    public void Update()
    {
        if (cooldown > 0)
            cooldown -= Time.deltaTime;
        else
            cooldown = 0;        

        if (isFiring)
        {
            windUp += Time.deltaTime;
            if (windUp >= baseWindUp)
            {
                windUp = baseWindUp;
                woundUp = true;
            }

            if(woundUp && cooldown <= 0)
            {
                if(baseChargeUp != 0 && chargeUp == 0)
                {
                    if (chargeUpEffect != null)
                    {
                        var spawnedChargeEffect = Instantiate(chargeUpEffect, transform.position, chargeUpEffect.transform.rotation * transform.rotation);
                        if (GameCoordinator.instance.currentSceneType != GameCoordinator.SceneType.Level)
                        {
                            foreach (var audio in spawnedChargeEffect.GetComponentsInChildren<AudioSource>())
                            {
                                audio.enabled = false;
                            }
                        }
                        if (GameCoordinator.instance.currentSceneType == GameCoordinator.SceneType.Shop)
                        { 
                            int portraitLayer = LayerMask.NameToLayer("PortraitRenderer");
                            foreach (var t in spawnedChargeEffect.GetComponentsInChildren<Renderer>())
                            {
                                t.gameObject.layer = portraitLayer;
                            }
                        }
                        spawnedChargeEffect.transform.parent = transform;
                        this.InvokeDelayed(3f, () => Destroy(spawnedChargeEffect));
                    }
                }

                chargeUp += Time.deltaTime;
                if (chargeUp >= baseChargeUp)
                    chargeUp = baseChargeUp;
            }
        }

        if (windDown > 0)
        {
            TryFire();

            windDown -= Time.deltaTime;
            if (windDown <= 0)
            {
                StopFiring();
            }
        }
    }
}

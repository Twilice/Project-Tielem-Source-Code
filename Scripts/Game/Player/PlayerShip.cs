using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvocationFlow;
using System;
using System.Linq;

public class PlayerShip : GameEntity
{
    public float baseSpeed;
    [NonSerialized]
    public float speed;
    public GameObject takeDamageExplosion;
    public GameObject takeHullDamageExplosion;
    public GameObject dashDepart;
    public GameObject dashArrive;
    [ReadOnlyInInspector]
    public List<Weapon> weapons; // todo :: make as "slots" so you can unregister / register a specific slot.
    [ReadOnlyInInspector]
    public PrimaryWeapon primaryWeapon;

    public float RatioHP => hp / maxHP;
    public float RatioShield => shield / maxShield;
    public float RatioEnergy => energy / maxEnergy;

    public float maxHP;
    public float maxShield;
    public float maxEnergy;
    [ReadOnlyInInspector]
    public float hp;
    [ReadOnlyInInspector]
    public float shield;
    public EnergyConsumer shieldEnergyConsumer;
    public float shieldRechargeAmount;

    [ReadOnlyInInspector]
    public float energy;
    public float energyRegeneration;

    //  *** dash *** 
    public float dashCooldown;
    public float dashLength;
    public float baseDashCooldown;
    public int maxDashes = 2;
    [ReadOnlyInInspector]
    public int dashes;

    public List<EnergyConsumer> energyConsumers = new List<EnergyConsumer>();
    // expected to be very small list, so don't really care about sorting it. Not even 100% if we should have the mecanic at all. So don't spend anymore time on it now.
    public List<EnergyConsumer> energyConsumers_inQueue = new List<EnergyConsumer>();

    private Camera mainCamera;
    public Vector3[] cameraCorners;
    void Awake()
    {
        InitBaseValues();

        shieldEnergyConsumer.Init(this);
        energyConsumers.Add(shieldEnergyConsumer);
        UpdatedEnergyNeedQueue(1 / 10f);

        mainCamera = Camera.main;
        cameraCorners = new Vector3[4];
        var distanceBetweenPlayerPlaneCamera = Vector3.Project(mainCamera.transform.position - transform.position, mainCamera.transform.forward).magnitude;
        mainCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), distanceBetweenPlayerPlaneCamera, Camera.MonoOrStereoscopicEye.Mono, cameraCorners);
        for (int i = 0; i < 4; i++)
        {
            cameraCorners[i] = mainCamera.transform.TransformVector(cameraCorners[i]) + mainCamera.transform.position;
        }
    }
    private void InitBaseValues()
    {
        hp = maxHP;
        energy = maxEnergy;
        shield = maxShield;
        speed = baseSpeed;
        dashes = maxDashes;
    }

    bool updateEnergyNeedQueue = false;
    void UpdatedEnergyNeedQueue(float interval)
    {
        updateEnergyNeedQueue = true;
        this.InvokeDelayed(interval, () => UpdatedEnergyNeedQueue(interval));
    }

    void Start()
    {
        if (weaponRegistered == false)
            FindAndRegisterWeaponsInPlayerShipHierarchy();

        // todo :: refactor all the orderby for readability and performance.
        energyConsumers = energyConsumers.OrderByDescending(x => x.priority).ToList();
    }

    // note :: Should only be used for test and demo without needing to use menu/coordinator
    private void FindAndRegisterWeaponsInPlayerShipHierarchy()
    {
        var primaryWeapon = GetComponentInChildren<PrimaryWeapon>();
        if (primaryWeapon != null)
        {
            RegisterPrimaryWeapon(primaryWeapon);
        }

        var weapons = gameObject.GetComponentsInChildren<Weapon>(depth: 1);
        foreach (var weapon in weapons)
        {
            if (weapon.transform.parent == transform)
            {
                RegisterWeapon(weapon);
            }
            // else it's primaryWeaponChild
        }
    }

    public bool weaponRegistered = false;
    public void RegisterWeapon(Weapon weapon)
    {
        weapons.Add(weapon);
        weaponRegistered = true;
        transform.AdoptChild(weapon.transform);
        weapon.owner = this;
        OnStartFiringListeners += weapon.OnStartFiring;
        OnStopFiringListeners += weapon.OnStopFiring;

        if (weapon.energyConsumer.IsUnused == false)
            energyConsumers.Add(weapon.energyConsumer);

        weapon.gameObject.SetActive(true);
    }

    public void RegisterPrimaryWeapon(PrimaryWeapon primaryWeapon)
    {
        this.primaryWeapon = primaryWeapon;
        weaponRegistered = true;

        transform.AdoptChild(primaryWeapon.transform);

        primaryWeapon.owner = this;
        OnStartFiringListeners += primaryWeapon.OnStartFiring;
        OnStopFiringListeners += primaryWeapon.OnStopFiring;

        foreach (var weapon in primaryWeapon.weaponSlots.SelectMany(slot => slot.weapons))
        {
            if (weapon.energyConsumer.IsUnused == false)
                energyConsumers.Add(weapon.energyConsumer);
        }

        primaryWeapon.gameObject.SetActive(true);
    }

    public void UnRegisterWeapon(Weapon weapon)
    {
        weapons.Remove(weapon);
        weapon.transform.parent = null;
        weapon.owner = null;
        OnStartFiringListeners -= weapon.OnStartFiring;
        OnStopFiringListeners -= weapon.OnStopFiring;

        if (weapon.energyConsumer.IsUnused == false)
            energyConsumers.Remove(weapon.energyConsumer);
    }

    public void UnRegisterPrimaryWeapon(PrimaryWeapon primaryWeapon)
    {
        this.primaryWeapon = null;
        primaryWeapon.owner = null;
        primaryWeapon.transform.parent = null;

        OnStartFiringListeners -= primaryWeapon.OnStartFiring;
        OnStopFiringListeners -= primaryWeapon.OnStopFiring;

        foreach (var weapon in primaryWeapon.weaponSlots.SelectMany(slot => slot.weapons))
        {
            if (weapon.energyConsumer.IsUnused == false)
                energyConsumers.Remove(weapon.energyConsumer);
        }

        primaryWeapon.gameObject.SetActive(false);
    }

    void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            Debug.DrawRay(mainCamera.transform.position, cameraCorners[i] - mainCamera.transform.position, Color.blue);
        }
        timeSinceDamageTaken += Time.deltaTime;
        if (isFiring)
            stoppedFiringArgs.TotalTimeFired += Time.deltaTime;
        UpdateEnergy();
        UpdateShield();
    }

    void LateUpdate()
    {
        if (GameCoordinator.instance.gameField == null) return;

        var bottomLeft = cameraCorners[0];
        var topLeft = cameraCorners[1];
        var topRight = cameraCorners[2];
        var bottomRight = cameraCorners[3];

        // todo :: make rotation independant

        if (transform.localPosition.x < bottomLeft.x)
            transform.localPosition = new Vector3(bottomLeft.x, transform.localPosition.y, transform.localPosition.z);
        if (transform.localPosition.x > topRight.x)
            transform.localPosition = new Vector3(topRight.x, transform.localPosition.y, transform.localPosition.z);
        if (transform.localPosition.z < bottomLeft.z)
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, bottomLeft.z);
        if (transform.localPosition.z > topRight.z)
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, topRight.z);
    }

    public bool rechargeDelay = false;
    void UpdateShield()
    {
        // only update 2 seconds after taking damage.
        if (rechargeDelay == false && shield < maxShield && timeSinceDamageTaken >= 2f)
        {
            if (shieldEnergyConsumer.HasEnoughEnergy())
            {
                shieldEnergyConsumer.UseEnergy();

                rechargeDelay = true;
                this.InvokeDelayed(2.0f, () => rechargeDelay = false);
                RechargeShield(0.3f, shieldRechargeAmount);
            }
        }
    }

    void RechargeShield(float time, float amount)
    {
        float rechargedShield = shieldRechargeAmount;
        this.InvokeWhile(() =>
        {
            rechargedShield -= shieldRechargeAmount * Time.deltaTime;
            shield += shieldRechargeAmount * Time.deltaTime;
            if (shield > maxShield)
            {
                shield = maxShield;
            }
        },
            () =>
            {
                return rechargedShield > 0 && shield <= maxShield;
            });
    }

    void UpdateEnergy()
    {
        energy += energyRegeneration * Time.deltaTime;

        energyConsumers.RemoveAll(x => x.owner == null); // remove all with destroyed owner
        var energyConsumersWithNeed = energyConsumers.Where(x => x.HasEnoughEnergy() == false);
        var totalNeed = energyConsumersWithNeed.Sum(x => x.need);

        // we have enough energy for everyone!
        if (totalNeed < energy)
        {
            foreach (var energyConsumer in energyConsumersWithNeed)
            {
                energyConsumer.energy += energyConsumer.need;
                energy -= energyConsumer.need;
            }
        }
        else if (updateEnergyNeedQueue) // energyConsumers_inQueue is expected to be very small list, so don't really care about sorting it. More important that it works as expected, hence the terrible while loops with MaxBy
        {
            updateEnergyNeedQueue = false;
            if (energyConsumers_inQueue.Count == 0)
            {
                foreach (var consumer in energyConsumersWithNeed)
                {
                    consumer.priority = consumer.basePriority;
                    energyConsumers_inQueue.Add(consumer);
                }
                energyConsumers_inQueue = energyConsumers_inQueue.OrderByDescending(x => x.priority).ToList();
            }
            else
            {
                energyConsumers_inQueue.RemoveAll(x => x.HasEnoughEnergy());
                var newConsumers = energyConsumersWithNeed.Where((consumer) => energyConsumers_inQueue.Contains(consumer) == false).OrderByDescending(x => x.priority).ToList();
                if (newConsumers.Any())
                {
                    // add new users
                    foreach (var newConsumer in newConsumers)
                    {
                        newConsumer.priority = newConsumer.basePriority;
                        energyConsumers_inQueue.Add(newConsumer);
                    }
                    energyConsumers_inQueue = energyConsumers_inQueue.OrderByDescending(x => x.priority).ToList();

                    // increase priority of users who will skip and wait for new users.
                    foreach (var newConsumer in newConsumers)
                    {
                        foreach (var x in energyConsumers_inQueue)
                        {
                            if (x.priority < newConsumer.priority)
                            {
                                x.priority++;
                            }
                        }
                    }
                }
            }
            // give to those with priority
            while (energyConsumers_inQueue.Count != 0)
            {
                var energyConsumer_inQueu = energyConsumers_inQueue.MaxBy(x => x.priority);
                if (energy < energyConsumer_inQueu.need)
                {
                    break; // no more energy
                }
                else
                {
                    energyConsumer_inQueu.energy += energyConsumer_inQueu.need;
                    energy -= energyConsumer_inQueu.need;
                    energyConsumers_inQueue.Remove(energyConsumer_inQueu);
                }
            }
        }

        if (maxEnergy < energy)
            energy = maxEnergy;
    }

    public float timeSinceDamageTaken;

    // todo :: figure out some fancy way of doing "InvokeNonDuplicate" that automatically creates and keeps track of state bool. Probably with ienumerator with yields e.g. waitforseconds for "escape" logic?
    bool preventDuplicateExplosion = false;
    private void SpawnTakeDamageExplosion()
    {
        if (takeDamageExplosion == null) return;
        if (preventDuplicateExplosion) return;

        preventDuplicateExplosion = true;
        var spawnedExplosion = Instantiate(takeDamageExplosion, transform.position, takeDamageExplosion.transform.rotation * transform.rotation);
        if (GameCoordinator.instance.currentSceneType != GameCoordinator.SceneType.Level)
        {
            foreach (var audio in spawnedExplosion.GetComponentsInChildren<AudioSource>())
            {
                audio.enabled = false;
            }
        }
        spawnedExplosion.transform.parent = transform;
        this.InvokeDelayed(0.2f, () => preventDuplicateExplosion = false);
        this.InvokeDelayed(0.5f, () => Destroy(spawnedExplosion));
    }

    bool preventDuplicateHullExplosion = false;
    private void SpawnTakeHullDamageExplosion()
    {
        if (takeHullDamageExplosion == null) return;
        if (preventDuplicateHullExplosion) return;

        preventDuplicateHullExplosion = true;
        var spawnedExplosion = Instantiate(takeHullDamageExplosion, transform.position, takeHullDamageExplosion.transform.rotation * transform.rotation);
        if (GameCoordinator.instance.currentSceneType != GameCoordinator.SceneType.Level)
        {
            foreach (var audio in spawnedExplosion.GetComponentsInChildren<AudioSource>())
            {
                audio.enabled = false;
            }
        }
        spawnedExplosion.transform.parent = transform;
        this.InvokeDelayed(0.2f, () => preventDuplicateHullExplosion = false);
        this.InvokeDelayed(0.5f, () => Destroy(spawnedExplosion));
    }

    public override void ApplyDamage(float amount, GameEntity source = null)
    {
        timeSinceDamageTaken = 0;

        if (shield > 0)
        {
            shield -= amount;
            SpawnTakeDamageExplosion();

            // overkill damage on shield should go to hp
            if (shield < 0)
            {
                hp += shield;
                shield = 0;
                SpawnTakeHullDamageExplosion();
            }
        }
        else
        {
            hp -= amount;
            SpawnTakeHullDamageExplosion();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // todo :: check for source and damage etc.
        ApplyDamage(10);

        var rgBody = other.attachedRigidbody;
        if (rgBody == null) return;
        var enemyShip = rgBody.GetComponent<EnemyShip>();

        if (enemyShip != null)
        {
            enemyShip.ApplyDamage(5);

            var displace = Vector3.ProjectOnPlane(transform.position - other.ClosestPoint(transform.position), transform.up);

            var previousDisplacement = Vector3.zero;
            this.TimeLerpValue(0.2f, Vector3.zero, displace, (displacement) =>
            {
                transform.position += displacement - previousDisplacement;
                previousDisplacement = displacement;
            });
        }
    }

    [NonSerialized]
    public bool isFiring = false;
    [NonSerialized]
    public bool isMoving = false;
    public void Fire()
    {
        if (Time.timeScale == 0) return;

        if (primaryWeapon != null)
            primaryWeapon.TryFire();

        foreach (var weapon in weapons)
            weapon.TryFire();
    }

    public void TryDash()
    {
        if (Time.timeScale == 0) return;

        if (0 < dashes)
        {
            Dash();
        }
    }

    [NonSerialized]
    public Vector3 dashDirection = Vector3.forward;
    public void Dash()
    {
        if (dashes == maxDashes)
        {
            this.InvokeWhileThen(() => dashCooldown -= Time.deltaTime, () => 0 < dashCooldown, () => dashes = maxDashes);
        }

        dashes--;
        dashCooldown = baseDashCooldown;

        this.TimeLerpValue(0.3f, 0, baseSpeed, (val) => speed = val);

        if (dashDepart != null)
        {
            var spawnedEffect = Instantiate(dashDepart, transform.position, Quaternion.LookRotation(transform.rotation * dashDepart.transform.rotation * -dashDirection));
            this.InvokeDelayed(1.5f, () => Destroy(spawnedEffect));
        }

        transform.position = transform.position + transform.rotation * dashDirection * dashLength;

        if (dashArrive != null)
        {
            var spawnedEffect = Instantiate(dashArrive, transform.position, Quaternion.LookRotation(transform.rotation * dashArrive.transform.rotation * -dashDirection));
            this.InvokeDelayed(1.5f, () => Destroy(spawnedEffect));
        }
    }

    public OnStoppedFiringEventArgs stoppedFiringArgs = new OnStoppedFiringEventArgs();
    public OnStopFiringHandler OnStopFiringListeners;
    public OnStartFiringHandler OnStartFiringListeners;


    public void PlayerMove(Vector3 movement)
    {
        transform.position += movement;
    }

    public void PlayerStartFire()
    {
        if (isFiring == false)
        {
            stoppedFiringArgs.TotalTimeFired = 0;
            OnStartFiringListeners?.Invoke(this);
            isFiring = true;
            this.InvokeWhile(Fire, () => isFiring);
        }
    }

    public void PlayerStopFire()
    {
        if (isFiring == true)
        {
            isFiring = false;
            OnStopFiringListeners?.Invoke(this, stoppedFiringArgs);
        }
    }
}
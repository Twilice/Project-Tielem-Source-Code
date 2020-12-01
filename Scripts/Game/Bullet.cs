using System.Collections;
using System.Collections.Generic;
using InvocationFlow;
using UnityEngine;

public class Bullet : GameEntity
{
    [Header("Set by owning weapon")]
    [ReadOnlyInInspector]
    public float lifeTime;
    [ReadOnlyInInspector]
    public float damage;
    public Vector3 velocity = new Vector3(0, 5);
    public bool attachToWeapon = false;

    public GameObject explosion;
    public float explosionDespawnTime = 3f;
    public bool explodeOnHit = true;

    private List<GameEntity> hitList;
    void Awake()
    {
        if (explodeOnHit == false)
            hitList = new List<GameEntity>();
    }

    private Vector3 offsetPos = Vector3.zero;
    void Update()
    {
        lifeTime -= Time.deltaTime;
        if(lifeTime <= 0)
        {
            Destroy(gameObject);
        }
        if (attachToWeapon)
        {
            offsetPos += velocity * Time.deltaTime;
            transform.position = owner.transform.position + offsetPos;
        }
        else
            transform.position = transform.position + velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        var rgBody = other.attachedRigidbody;
        if (rgBody == null) return;
        var entity = rgBody.GetComponent<GameEntity>();
        if (entity != null)
        {
            if (explodeOnHit)
            {
                this.InvokeDelayed(0, () => Destroy(gameObject));
                entity.ApplyDamage(damage);

                if (explosion != null)
                {
                    var spawnedExplosion = Instantiate(explosion, transform.position, explosion.transform.rotation * transform.rotation);
                    if (GameCoordinator.instance.currentSceneType != GameCoordinator.SceneType.Level)
                    {
                        foreach (var audio in spawnedExplosion.GetComponentsInChildren<AudioSource>())
                        {
                            audio.enabled = false;
                        }
                    }
                    Flow.InvokeDelayed(explosionDespawnTime, () => Destroy(spawnedExplosion));
                }

            }
            else
            {
                if (hitList.Contains(entity) == false)
                {
                    hitList.Add(entity);
                    entity.ApplyDamage(damage);

                    if (explosion != null)
                    {
                        var spawnedExplosion = Instantiate(explosion, other.attachedRigidbody.transform.position, explosion.transform.rotation * transform.rotation);
                        if (GameCoordinator.instance.currentSceneType != GameCoordinator.SceneType.Level)
                        {
                            foreach (var audio in spawnedExplosion.GetComponentsInChildren<AudioSource>())
                            {
                                audio.enabled = false;
                            }
                        }
                        Flow.InvokeDelayed(explosionDespawnTime, () => Destroy(spawnedExplosion));
                    }
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using InvocationFlow;
using UnityEngine;

public class EnemyShip : GameEntity
{
    public Weapon[] weapons;
    public GameObject explosion;
    public Vector3 velocity;
    public float offsetCurveTime;
    public AnimationCurve offsetCurveX;
    public AnimationCurve offsetCurveY;
    public AnimationCurve offsetCurveZ;
    public float delayOffsetMovement = 2.0f;
    public float delayAIInit = 1.0f;

    public float hp = 3;

    void Start()
    {
        weapons = GetComponentsInChildren<Weapon>();
        this.InvokeDelayed(delayAIInit, initAILogic);
    }

    void initAILogic()
    {
        if (offsetCurveX.length + offsetCurveY.length + offsetCurveZ.length == 0)
        {
            this.InvokeDelayed(Random.Range(0f, 2f), () => this.InvokeWhile(() => { foreach (var weapon in weapons) weapon.TryFire(); }, () => true));
            this.InvokeWhile(Move, () => true);
        }
        else
        {
            float delayTime = delayOffsetMovement;
            this.InvokeWhileThen(
                Move,
                () =>
                {
                    delayTime -= Time.deltaTime;
                    return delayTime > 0;
                },
                () =>
                {
                    noOffsetPosition = transform.position;
                    RepeatTryFireRandomDelay(0.5f, 4.0f);
                    this.TimeLerpValueThen(offsetCurveTime, 0, 1, MoveOffset, () => this.InvokeWhile(Move, () => true));
                });
        }
    }

    void RepeatTryFireRandomDelay(float min, float max)
    {
        this.InvokeDelayed(Random.Range(min, max), () => { foreach (var weapon in weapons) weapon.TryFire(); RepeatTryFireRandomDelay(min, max); });

    }


    void Move()
    {
        transform.position = transform.position + velocity * Time.deltaTime;
    }

    private Vector3 noOffsetPosition;
    void MoveOffset(float evaluateTime)
    {
        noOffsetPosition += velocity * Time.deltaTime;
        transform.position = noOffsetPosition + new Vector3(offsetCurveX.Evaluate(evaluateTime), offsetCurveY.Evaluate(evaluateTime), offsetCurveZ.Evaluate(evaluateTime));
    }

    public override void ApplyDamage(float damage, GameEntity source = null)
    {
        hp -= damage;
        if (hp <= 0)
        {
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
                Flow.InvokeDelayed(3f, () => Destroy(spawnedExplosion));
            }
            Destroy(gameObject);
        }
    }
}

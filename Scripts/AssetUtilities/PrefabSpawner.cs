using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvocationFlow;

public class PrefabSpawner : MonoBehaviour
{
    public GameObject prefab;
    public float startSpawnDelay;
    public float minTime;
    public float maxTime;

    void Start()
    {
        this.InvokeDelayed(startSpawnDelay, () =>
        {
            if (minTime == maxTime)
            {
                Spawn();
            }
            else
            {
                SpawnRandom();
            }
        });
    }

    void Spawn()
    {
        Instantiate(prefab, transform.position, transform.rotation);
        this.InvokeDelayed(minTime, Spawn);
    }


    void SpawnRandom()
    {
        Instantiate(prefab, transform.position, transform.rotation);
        this.InvokeDelayed(Random.Range(minTime, maxTime), SpawnRandom);
    }
}

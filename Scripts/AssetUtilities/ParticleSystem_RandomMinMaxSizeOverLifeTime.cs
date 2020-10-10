using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystem_RandomMinMaxSizeOverLifeTime : MonoBehaviour
{
    public List<ParticleSystem> particleSystems;
    public List<ParticleSystem.MinMaxCurve> curves;

    void Start()
    {
        if (curves == null || curves.Count == 0) return;
        if (particleSystems == null || particleSystems.Count == 0) return;

        var curve = curves[Random.Range(0, curves.Count)];
        foreach(var ps in particleSystems)
        {
            var module = ps.sizeOverLifetime;
            module.size = curve;
        }
    }
}

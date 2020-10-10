using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnergyConsumer
{
    public EnergyConsumer(float need, float totalCost, float basePriority, GameEntity owner)
    {
        this.need = need;
        this.totalCost = totalCost;
        this.basePriority = basePriority;
        this.owner = owner;
        energy = totalCost;
        priority = 0;
    }

    public void Init(GameEntity owner)
    {
        energy = totalCost;
        this.owner = owner;
        priority = 0;
    }

    public float need;
    public float totalCost;
    public float basePriority;
    public float priority;
    public float energy;
    public GameEntity owner;
    public bool HasEnoughEnergy()
    {
        return totalCost <= energy;
    }

    public void UseEnergy()
    {
        energy -= totalCost;
    }

    public bool IsUnused => need == 0;
}

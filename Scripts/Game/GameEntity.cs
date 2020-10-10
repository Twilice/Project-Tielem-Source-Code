using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntity : MonoBehaviour
{
    [ReadOnlyInInspector]
    public GameEntity owner;
    [ReadOnlyInInspector]
    public GameEntity[] childs;

    public virtual void ApplyDamage(float damage, GameEntity source = null)
    {
        Debug.LogWarning($"{name} received {damage} damage from {source.name} but has no override for ApplyDamage.");
    }

    public delegate void OnStartFiringHandler(GameEntity sender);

    public delegate void OnStopFiringHandler(GameEntity sender, OnStoppedFiringEventArgs args);
    public class OnStoppedFiringEventArgs : EventArgs
    {
        public float TotalTimeFired { get; set; }
    }
}

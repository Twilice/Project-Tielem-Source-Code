using System.Collections;
using System.Collections.Generic;
using InvocationFlow;
using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
    public float proximinityWakeUpGroup;
    public EnemyShip[] units;
    public float awakeInterval;

    void Awake()
    {
        units = GetComponentsInChildren<EnemyShip>();
        foreach(var unit in units)
        {
            unit.gameObject.SetActive(false);
        }

        this.InvokeWhen(WakeUpUnits, InProximity);
    }

    void WakeUpUnits()
    {
        if (awakeInterval == 0)
        {
            foreach (var unit in units)
            {
                unit.gameObject.SetActive(true);
            }
        }
        else
        {
            this.InvokeDelayed(awakeInterval, () => WakeUpUnit(0));
        }
    }

    void WakeUpUnit(int i)
    {
        if (i < units.Length)
        {
            units[i].gameObject.SetActive(true);
        }
        i++;
        if (i < units.Length)
        {
            this.InvokeDelayed(awakeInterval, () => WakeUpUnit(i));
        }
    }

    bool InProximity()
    {
        return transform.position.z - GameField.Field.transform.position.z < proximinityWakeUpGroup;
    }
}

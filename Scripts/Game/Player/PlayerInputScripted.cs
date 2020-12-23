using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvocationFlow;


// note :: this is a kinda bad implementation of sequence. Will have to do for now since the weapon preview is very simple.
[System.Serializable]
public class ScriptedInputSequence
{
    public enum InputType
    {
        None,
        MoveDelta,
        SetPosition,
        StartFire,
        StopFire
    }
    public InputType inputType = InputType.None;
    public float duration = 0;
    public Vector3 movement = Vector3.zero;
    public float delayNext = 0;
    public bool awaitRunningSequence = false;
}


public class PlayerInputScripted : MonoBehaviour
{
    public float delayInitialSequence = 0;
    public bool loop = false;
    public PlayerShip playerShip;
    [ReadOnlyInInspector]
    public int currentIndex = -1;
    public List<ScriptedInputSequence> currentRunning;
    public List<ScriptedInputSequence> scriptedInputSequence;

    void Awake()
    {
        if (playerShip == null)
        {
            playerShip = FindObjectOfType<PlayerShip>();
        }
        this.InvokeDelayed(delayInitialSequence, () => ExecuteNextSequence());
    }

    void ExecuteNextSequence()
    {
        var next = GetNextSequence();
        if (currentIndex + 1 < scriptedInputSequence.Count)
        {
            currentIndex++;
        }
        else if (loop)
        {
            currentIndex = 0;
        }

        switch (next.inputType)
        {
            case ScriptedInputSequence.InputType.None:
                break;
            case ScriptedInputSequence.InputType.MoveDelta:
                Vector3 previousDelta = Vector3.zero;
                this.TimeLerpValue(next.duration, Vector3.zero, next.movement, (totalDelta) => {
                        playerShip.PlayerMove(totalDelta - previousDelta);
                        previousDelta = totalDelta;
                    });
                break;
            case ScriptedInputSequence.InputType.SetPosition:
                this.TimeLerpValue(next.duration, playerShip.transform.position, next.movement, (position) => playerShip.transform.position = position);
                break;
            case ScriptedInputSequence.InputType.StartFire:
                playerShip.PlayerStartFire();
                if (next.duration != 0)
                    this.InvokeDelayed(next.duration, 
                        () =>
                        {
                            this.InvokeDelayed(() =>
                            {
                                playerShip.PlayerStopFire();
                            });
                        });
                break;
            case ScriptedInputSequence.InputType.StopFire:
                playerShip.PlayerStopFire();
                if (next.duration != 0)
                    this.InvokeDelayed(next.duration, () => playerShip.PlayerStartFire());
                break;
            default:
                break;
        }

        if (next.duration != 0)
        {
            currentRunning.Add(next);
            this.InvokeDelayed(next.duration, () => currentRunning.Remove(next));
        }

        var afterNext = GetNextSequence();

        if (next.delayNext != 0 && afterNext.awaitRunningSequence)
            this.InvokeWhen(() => this.InvokeDelayed(next.delayNext, () => ExecuteNextSequence()), () => currentRunning.Count == 0);
        else if (afterNext.awaitRunningSequence)
            this.InvokeWhen(() => ExecuteNextSequence(), () => currentRunning.Count == 0);
        else if (next.delayNext != 0)
            this.InvokeDelayed(next.delayNext, () => ExecuteNextSequence());
        else
            ExecuteNextSequence();
    }

    ScriptedInputSequence GetNextSequence()
    {
        if (currentIndex + 1 < scriptedInputSequence.Count)
        {
            return scriptedInputSequence[currentIndex + 1];
        }
        else return scriptedInputSequence[0];
    }
}

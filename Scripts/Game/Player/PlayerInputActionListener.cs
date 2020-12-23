using System;
using InvocationFlow;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputActions;

public class PlayerInputActionListener : MonoBehaviour, IPlayerActions
{
    public PlayerShip playerShip;
    public InputActions inputActions;

    void Awake()
    {
        if (playerShip == null)
        {
            playerShip = FindObjectOfType<PlayerShip>();
        }
        inputActions = new InputActions();
        inputActions.Player.SetCallbacks(this);
        inputActions.Player.Enable();
    }

    void OnDestroy()
    {
        if (inputActions != null)
            inputActions.Dispose();
    }

    void Update()
    {
        Move();
    }

    public void Move()
    {
        Vector2 playerInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(playerInput.x, 0, playerInput.y);
        if (0.1 < moveDirection.sqrMagnitude)
        {
            playerShip.dashDirection = moveDirection;
        }

        playerShip.PlayerMove(playerShip.transform.rotation * moveDirection * playerShip.speed * Time.deltaTime);
    }

    void IPlayerActions.OnFire(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:
                playerShip.PlayerStartFire();
                break;
            case InputActionPhase.Canceled:
                playerShip.PlayerStopFire();
                break;
            default:
                throw new NotSupportedException("Input Action needs to have it's phase duh...");
        }
        //throw new System.NotImplementedException();
    }

    void IPlayerActions.OnLook(InputAction.CallbackContext context)
    {
        //Debug.Log("OnLook");
        //throw new System.NotImplementedException();
    }

    // can't use this as planned with invocationFlow... canceled is fired just before a new performed meaning there will be 2 flows.
    void IPlayerActions.OnMove(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0) return;

        //switch (context.phase)
        //{
        //    case InputActionPhase.Disabled:
        //        break;
        //    case InputActionPhase.Waiting:
        //        break;
        //    case InputActionPhase.Started:
        //        break;
        //    case InputActionPhase.Performed:
        //        if(isMoving == false)
        //        {
        //            isMoving = true;
        //            this.InvokeWhile(Move, () => isMoving);
        //        }
        //        break;
        //    case InputActionPhase.Canceled:
        //        isMoving = false;
        //        break;
        //    default:
        //        throw new NotSupportedException("Input Action needs to have it's phase duh...");
        //}
        //throw new System.NotImplementedException();
    }

    void IPlayerActions.OnDash(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:
                playerShip.TryDash();
                break;
            case InputActionPhase.Canceled:
                break;
            default:
                throw new NotSupportedException("Input Action needs to have it's phase duh...");
        }
    }
}

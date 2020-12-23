using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTilt : MonoBehaviour
{
    private Vector3 previousPosition = Vector3.zero;
    private Animator animator;
    private static int parameterId;
    public string animationName = "Direction";

    public float playIdle1Time = 5f;
    public float idle1Cooldown = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (GameCoordinator.instance.currentSceneType == GameCoordinator.SceneType.None)
        {
            this.enabled = false;
            return;
        }


        animator = GetComponent<Animator>();
        parameterId = Animator.StringToHash("Direction");
    }


    // Update is called once per frame
    void Update()
    {

        // todo :: also use rotation in case we use other axis as base.
        var direction = transform.position.x - previousPosition.x;


        if (direction > 0)
        {
            direction = 1;
        }
        else if (direction < 0)
        {
            direction = -1;
        }

        if (GameCoordinator.instance.currentSceneType == GameCoordinator.SceneType.Level)
        { 
            if (idle1Cooldown <= 0 && GameCoordinator.instance.playerShip.idleTime > playIdle1Time)
            {
                animator.SetTrigger("Idle1");
                idle1Cooldown = playIdle1Time;
            }
            else
            {
                idle1Cooldown -= Time.deltaTime;
            }
        }

        animator.SetFloat(parameterId, direction);

        previousPosition = transform.position;
    }
}

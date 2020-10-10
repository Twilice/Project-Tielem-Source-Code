using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePosition : MonoBehaviour
{
    public Vector3 moveVector;
    public Quaternion vectorSpace = Quaternion.identity;
    public float disableMoveAfterSeconds = 0;
    public float deactiveGameObjectAfterSeconds = 0;

    void Start()
    {
        
        if (disableMoveAfterSeconds != 0)
            Invoke("DisableMovePosition", disableMoveAfterSeconds);

        if (disableMoveAfterSeconds != 0)
            Invoke("DisableGameObjectPosition", deactiveGameObjectAfterSeconds);
    }

    void DisableMovePosition()
    {
        this.enabled = false;
    }

    void DisableGameObjectPosition()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        transform.position = transform.position + vectorSpace * transform.rotation * moveVector * Time.deltaTime;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
    public static GameField Field;
    public Vector3 velocity;
    private void Awake()
    {
        if(Field != null)
        {
            throw new System.Exception("Multiple GameFields present in the scene.");
        }
        Field = this;
    }

    void Update()
    {
        transform.position = transform.position + velocity * Time.deltaTime;
    }
}

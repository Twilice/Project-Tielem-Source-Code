using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotator : MonoBehaviour {

    public float speed = 100;
    public Vector3 direction = Vector3.forward;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        transform.rotation *= Quaternion.AngleAxis(speed * Time.deltaTime, direction);
	}
}

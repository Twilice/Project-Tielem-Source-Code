using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpdateShaderProperty : MonoBehaviour
{
    public string valuePropertyName;
    public string shaderPropertyName;
    [ReadOnlyInInspector] public PlayerShip playerShip;
    private Material mat;
    private int shaderPropertyId;
    private Func<float> getValueProperty;
    
    void Start()
    {
        if(playerShip == null)
            playerShip = FindObjectOfType<PlayerShip>();
        mat = new Material(GetComponent<Image>().material); // create new instance
        GetComponent<Image>().material = mat;
        shaderPropertyId = Shader.PropertyToID(shaderPropertyName);
        var getAccessor = playerShip.GetType().GetProperty(valuePropertyName).GetGetMethod();
        getValueProperty = (Func<float>) Delegate.CreateDelegate(typeof(Func<float>), playerShip, getAccessor, true);
    }

    void Update()
    {
        mat.SetFloat(shaderPropertyId, getValueProperty());
    }
}

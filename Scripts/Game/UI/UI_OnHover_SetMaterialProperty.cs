using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_OnHover_SetMaterialProperty : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string shaderPropertyName = "_EffectStrength";
    public int effectStrength = 1;
    public List<Image> images = new List<Image>();
    private List<(Material material, int propertyId)> materialProperties = new List<(Material, int)>();

    void Start()
    {
        foreach (var image in images)
        {
            var mat = new Material(image.material); // create new instance
            image.material = mat;
            var propertyId = Shader.PropertyToID(shaderPropertyName);
            mat.SetFloat(propertyId, 0);
            materialProperties.Add((mat, Shader.PropertyToID(shaderPropertyName)));
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var materialProperty in materialProperties)
        {
            materialProperty.material.SetFloat(materialProperty.propertyId, effectStrength);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (var materialProperty in materialProperties)
        {
            materialProperty.material.SetFloat(materialProperty.propertyId, 0);
        }
    }
}

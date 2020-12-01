using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_OnHover_SetAnimationTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string enterTrigger = "OnHover";
    public string exitTrigger = "OnStopHover";

    public Animator animator;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetTrigger(enterTrigger);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetTrigger(exitTrigger);
    }
}

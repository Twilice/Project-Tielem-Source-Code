using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StartLevel : MonoBehaviour, IPointerClickHandler
{
    // temp: see GameCoordinator. StartLevel comment.
    public int tempLevelStart = 0;
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (tempLevelStart != 0)
            GameCoordinator.instance.StartLevel(tempLevelStart);
        else
            GameCoordinator.instance.StartLevel();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ActionButton : MonoBehaviour
{
    public string methodName;
    [Header("Set null to reference GameCoordinator")]
    [Tooltip("The script listening on ActionButton")]
    public MonoBehaviour controllerType;

    void Start()
    {
        if(controllerType == null)
        {
            controllerType = GameCoordinator.instance;
        }

        // Note :: a more strict/controlled way instead of Component.SendMessage in case we might have a future returnvalue of bool or action.
        var methodInfo = controllerType.GetType().GetMethod(methodName);
        if (methodInfo == null)
        {
            Debug.LogError($"No public method named {methodName} in {controllerType.name}. Failed binding to {nameof(ActionButton)} - {transform.parent.name}/{name}");
            return;
        }

        if(methodInfo.GetParameters().Length > 0)
        {
            var createdDelegate = methodInfo.CreateDelegate(typeof(Action<GameObject>), controllerType);

            if (createdDelegate is Action<GameObject> createdAction)
            {
                var button = GetComponent<Button>();
                UnityAction onClickLambda = () => createdAction(gameObject);
                button.onClick.AddListener(onClickLambda);
            }
        }
        else
        {
            var createdDelegate = methodInfo.CreateDelegate(typeof(Action), controllerType);

            if (createdDelegate is Action createdAction)
            {
                var button = GetComponent<Button>();
                UnityAction lambda = () => createdAction();
                button.onClick.AddListener(lambda);
            }
        }

        //test sendmessagetest to controller with no update - works in case we want lazy way out with arguments
        //controllerType.SendMessage(methodName);
    }
}

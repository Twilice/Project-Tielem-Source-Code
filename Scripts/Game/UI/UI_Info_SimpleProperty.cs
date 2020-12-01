using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextCompositeString))]
public class UI_Info_SimpleProperty : MonoBehaviour
{
#if UNITY_EDITOR
    public enum SupportedPropertyTypes
    {
        @int,
        @float,
        @string,
    }

    [Header("List of supported types. \"readonly\"")]
    [Tooltip("Setting this field doesn't actually do anything. Just lazy way of showing supported properties.")]
    public SupportedPropertyTypes SupportedProprties;
#endif

    public bool zeroEqualsNull = false;
    public string propertyName;
    [Header("Set null to reference GameCoordinator")]
    [Tooltip("The script to fetch property value from")]
    public MonoBehaviour dataSource;

    TextCompositeString textCompositeString;
    public float textRefreshRate = 0.05f;
    private float timeUntilNextRefresh = 0;

    private Func<object> cached_getPropertyValue;
    void Start()
    {
        textCompositeString = GetComponent<TextCompositeString>();

        if (dataSource == null)
        {
            dataSource = GameCoordinator.instance;
        }

        var propertyInfo = dataSource.GetType().GetProperty(propertyName);
        if (propertyInfo == null)
        {
            Debug.LogError($"No property named {propertyName} in {dataSource.name}. Failed binding to {nameof(UI_Info_SimpleProperty)} - {transform.parent.name}/{name}");
            return;
        }

        var methodInfo = propertyInfo.GetGetMethod();

        // note :: since we know the propertyType we can just fetch the actual get method and call it directly. Since we only use reflection to get the method, not to invoke it. It's actually fast!
        var delegateType = typeof(Func<>).MakeGenericType(propertyInfo.PropertyType);
        var createdDelegate = methodInfo.CreateDelegate(delegateType, dataSource);

        // note :: to properly cast the function signature we need to define a set of supported types.
        #region convert delegate to boxed value func<object> to use by string.format
        if (propertyInfo.PropertyType == typeof(int))
        {
            var getInt = (Func<int>)createdDelegate;
            if (zeroEqualsNull)
            {
                cached_getPropertyValue = () =>
                {
                    int val = getInt();
                    if (val == 0)
                        return null;
                    else return val;
                };
            }
            else
            {
                cached_getPropertyValue = () => { return getInt(); };
            }
        }
        else if (propertyInfo.PropertyType == typeof(float))
        {
            var getFloat = (Func<float>)createdDelegate;
            if (zeroEqualsNull)
            {
                cached_getPropertyValue = () =>
                {
                    float val = getFloat();
                    if (val == 0)
                        return null;
                    else return val;
                };
            }
            else
            {
                cached_getPropertyValue = () => { return getFloat(); };
            }
        }
        else if (propertyInfo.PropertyType == typeof(string))
        {
            var getString = (Func<string>)createdDelegate;
            cached_getPropertyValue = () => { return getString(); };
        }
        else
        {
            Debug.LogError($"Type {propertyInfo.PropertyType} not supported for {nameof(UI_Info_SimpleProperty)}");
            throw new NotSupportedException($"Type {propertyInfo.PropertyType} not supported for {nameof(UI_Info_SimpleProperty)}");
        }
        #endregion

        if(textRefreshRate == -1)
        {
            textCompositeString.UpdateArgument(cached_getPropertyValue());
            this.enabled = false;
        }
    }

    void LateUpdate()
    {
        if (timeUntilNextRefresh <= 0)
        {
            textCompositeString.UpdateArgument(cached_getPropertyValue());
            timeUntilNextRefresh = textRefreshRate;
        }
        timeUntilNextRefresh -= Time.deltaTime;
    }
}

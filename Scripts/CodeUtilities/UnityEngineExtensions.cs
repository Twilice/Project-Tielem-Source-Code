using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UnityEngine
{
    public static class UnityEngineUtilities
    {
        /// <summary>
        /// Set parent transform of child to this transform without updating child local transform data.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public static void AdoptChild(this Transform parent, Transform child)
        {
            var localPos = child.localPosition;
            var localRot = child.localRotation;
            var localScale = child.localScale;
            child.parent = parent;
            child.localPosition = localPos;
            child.localRotation = localRot;
            child.localScale = localScale;
        }

        public static bool NotRefNull(UnityEngine.Object obj)
        {
            return ReferenceEquals(obj, null) == false;
        }

        /// <summary>
        /// Find component in children, returns true if found else false. Note that component will be null if not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="component">Reference to found component</param>
        /// <returns></returns>
        public static bool TryFindComponentInChildren<T>(this GameObject go, ref T component)
        {
            component = go.GetComponentInChildren<T>();
            return component != null;
        }

        public static T[] GetComponentsInChildren<T>(this GameObject go, bool includeInactive, int depth) where T : MonoBehaviour
        {
            var childs = go.GetComponentsInChildren<T>(includeInactive);
            return childs.Where(x => GenerationDistance(go.transform, x.transform) <= depth).ToArray();
        }

        public static  void GetComponentsInChildren<T>(this GameObject go, bool includeInactive, List<T> result, int depth) where T : MonoBehaviour
        {
            go.GetComponentsInChildren(includeInactive, result);
            result = result.Where(x => GenerationDistance(go.transform, x.transform) <= depth).ToList();
        }

        public static T[] GetComponentsInChildren<T>(this GameObject go, int depth) where T : MonoBehaviour
        {
            var childs = go.GetComponentsInChildren<T>(includeInactive: false);
            return childs.Where(x => GenerationDistance(go.transform, x.transform) <= depth).ToArray();
        }
       
        public static void GetComponentsInChildren<T>(this GameObject go, List<T> results, int depth) where T : MonoBehaviour
        {
            go.GetComponentsInChildren(includeInactive: false, results);
            results = results.Where(x => GenerationDistance(go.transform, x.transform) <= depth).ToList();
        }

        /// <summary>
        /// Calculates the parent child relation distance in hierarchy. Returns 0 if they are the same object. Returns -1 if they are not related.
        /// </summary>
        /// <param name="elder"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static int GenerationDistance(Transform elder, Transform child)
        {
            if (elder == child) return 0;
            return _GenerationDistance(elder, child, 1);
            int _GenerationDistance(Transform _elder, Transform _child, int dist)
            {
                if (_child.parent == _elder)
                    return dist;
                else if (_child.parent == _child.root)
                    return -1;
                else return _GenerationDistance(_elder, _child.parent, dist + 1);
            }
        }
    }

    public static class VectorExtensions
    {
        /// <summary>
        /// Returns a copy of invoking vector with an offset to the position.
        /// </summary>
        /// <param name="x"> The x offset</param>
        /// <param name="y"> The y offset</param>
        /// <param name="z"> The z offset</param>
        public static Vector3 CopyWithOffset(this Vector3 vec3, float x, float y, float z)
        {
            vec3.x += x;
            vec3.y += y;
            vec3.z += z;

            return vec3;
        }
    }

    public static class ColorExtensions
    {
        /// <summary>
        /// Returns a copy of invoking color with a new alpha.
        /// </summary>
        /// <param name="value"> The new alpha</param>
        public static Color CopyWithAlpha(this Color color, float value)
        {
            color.a = value;

            return color;
        }
    }
}

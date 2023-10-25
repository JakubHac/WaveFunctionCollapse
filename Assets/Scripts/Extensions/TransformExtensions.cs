using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class TransformExtensions
{
    public static void DestroyChildren<T>(this Transform transform, Action<T> beforeDestroy = null) where T : Component
    {
        foreach (var child in transform.GetComponentsInChildren<T>())
        {
            beforeDestroy?.Invoke(child);
            Object.Destroy(child.gameObject);
        }
    }
}

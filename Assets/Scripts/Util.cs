﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Note: Reused from Lukas' HackerJedi project: https://github.com/Bone008/HackerJedi/blob/master/Assets/Scripts/Util.cs
public static class Util
{
    public static float EaseInOut01(float t)
    {
        return Mathf.SmoothStep(0, 1, t);
    }


    public static Coroutine Animate<T>(this MonoBehaviour component, float duration, T from, T to, Func<T, T, float, T> interpolationFunc, Action<T> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, progress => valueCallback(interpolationFunc(from, to, progress)), useRealtime);
    }
    public static Coroutine Animate<T>(this MonoBehaviour component, float duration, T from, T to, Func<T, T, float, T> interpolationFunc, AnimationCurve curve, Action<T> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, curve.Evaluate, progress => valueCallback(interpolationFunc(from, to, progress)), useRealtime);
    }
    public static Coroutine Animate<T>(this MonoBehaviour component, float duration, T from, T to, Func<T, T, float, T> interpolationFunc, Func<float, float> easingFunc, Action<T> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, easingFunc, progress => valueCallback(interpolationFunc(from, to, progress)), useRealtime);
    }

    public static Coroutine AnimateScalar(this MonoBehaviour component, float duration, float from, float to, Action<float> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, progress => valueCallback(Mathf.Lerp(from, to, progress)), useRealtime);
    }
    public static Coroutine AnimateScalar(this MonoBehaviour component, float duration, float from, float to, AnimationCurve curve, Action<float> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, curve.Evaluate, progress => valueCallback(Mathf.Lerp(from, to, progress)), useRealtime);
    }
    public static Coroutine AnimateScalar(this MonoBehaviour component, float duration, float from, float to, Func<float, float> easingFunc, Action<float> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, easingFunc, progress => valueCallback(Mathf.Lerp(from, to, progress)), useRealtime);
    }

    public static Coroutine AnimateVector(this MonoBehaviour component, float duration, Vector3 from, Vector3 to, Action<Vector3> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, progress => valueCallback(Vector3.Lerp(from, to, progress)), useRealtime);
    }
    public static Coroutine AnimateVector(this MonoBehaviour component, float duration, Vector3 from, Vector3 to, AnimationCurve curve, Action<Vector3> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, curve.Evaluate, progress => valueCallback(Vector3.Lerp(from, to, progress)), useRealtime);
    }
    public static Coroutine AnimateVector(this MonoBehaviour component, float duration, Vector3 from, Vector3 to, Func<float, float> easingFunc, Action<Vector3> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, easingFunc, progress => valueCallback(Vector3.Lerp(from, to, progress)), useRealtime);
    }

    public static Coroutine AnimateQuaternion(this MonoBehaviour component, float duration, Quaternion from, Quaternion to, Action<Quaternion> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, progress => valueCallback(Quaternion.Slerp(from, to, progress)), useRealtime);
    }
    public static Coroutine AnimateQuaternion(this MonoBehaviour component, float duration, Quaternion from, Quaternion to, AnimationCurve curve, Action<Quaternion> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, curve.Evaluate, progress => valueCallback(Quaternion.Slerp(from, to, progress)), useRealtime);
    }
    public static Coroutine AnimateQuaternion(this MonoBehaviour component, float duration, Quaternion from, Quaternion to, Func<float, float> easingFunc, Action<Quaternion> valueCallback, bool useRealtime = false)
    {
        return Animate(component, duration, easingFunc, progress => valueCallback(Quaternion.Slerp(from, to, progress)), useRealtime);
    }

    public static Coroutine Animate(this MonoBehaviour component, float duration, Action<float> progressCallback, bool useRealtime = false)
    {
        return Animate(component, duration, t => t, progressCallback, useRealtime);
    }
    public static Coroutine Animate(this MonoBehaviour component, float duration, AnimationCurve curve, Action<float> progressCallback, bool useRealtime = false)
    {
        return component.StartCoroutine(_AnimateCoroutine(duration, curve.Evaluate, progressCallback, useRealtime));
    }
    public static Coroutine Animate(this MonoBehaviour component, float duration, Func<float, float> easingFunc, Action<float> progressCallback, bool useRealtime = false)
    {
        return component.StartCoroutine(_AnimateCoroutine(duration, easingFunc, progressCallback, useRealtime));
    }

    private static IEnumerator _AnimateCoroutine(float duration, Func<float, float> easingFunc, Action<float> progressCallback, bool realtime)
    {
        float t = 0;
        while(t < duration)
        {
            progressCallback(easingFunc(t / duration));
            yield return null;
            t += (realtime ? Time.unscaledDeltaTime : Time.deltaTime);
        }
        progressCallback(1.0f);
    }



    /// <summary>
    /// A thin wrapper around StartCoroutine for simple scenarios where you just want to execute something after some time has passed.
    /// </summary>
    /// <param name="component">the script that wants to delay something</param>
    /// <param name="delay">delay in ingame seconds</param>
    /// <param name="callback">the callback that should be invoked; useful with lambda expressions: () => ...</param>
    /// <returns>the coroutine as created by Unity</returns>
    public static Coroutine Delayed(this MonoBehaviour component, float delay, Action callback)
    {
        return component.StartCoroutine(_DelayedCoroutine(new WaitForSeconds(delay), callback));
    }


    /// <summary>
    /// A thin wrapper around StartCoroutine for simple scenarios where you just want to execute something after some time has passed.
    /// </summary>
    /// <param name="component">the script that wants to delay something</param>
    /// <param name="delay">something you could "yield return" from a coroutine</param>
    /// <param name="callback">the callback that should be invoked; useful with lambda expressions: () => ...</param>
    /// <returns>the coroutine as created by Unity</returns>
    public static Coroutine Delayed(this MonoBehaviour component, YieldInstruction delayObject, Action callback)
    {
        return component.StartCoroutine(_DelayedCoroutine(delayObject, callback));
    }


    private static IEnumerator _DelayedCoroutine(YieldInstruction delayObject, Action callback)
    {
        yield return delayObject;
        callback();
    }
    
    /// <summary>
    /// Searches for a GameObject with a specific tag in the current gameobject and its parents
    /// </summary>
    /// <param name="current">the current gameobject</param>
    /// <param name="searchedTag">the name of the tag that is searched for</param>
    /// <returns>the GameObject with the specified tag or null if the search was unsuccessful</returns>
    public static GameObject GetGoInParentWithTag(this GameObject current, string searchedTag)
    {
        Transform currentParent = current.transform;

        while(currentParent != null)
        {
            if (currentParent.tag.Equals(searchedTag))
                return currentParent.gameObject;

            currentParent = currentParent.parent;
        }

        return null;
    }






    // MinBy for Linq
    // source: http://stackoverflow.com/a/914198

    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
    Func<TSource, TKey> selector)
    {
        return source.MinBy(selector, null);
    }

    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector, IComparer<TKey> comparer)
    {
        if (source == null) throw new ArgumentNullException("source");
        if (selector == null) throw new ArgumentNullException("selector");
        comparer = comparer ?? Comparer<TKey>.Default;

        using (var sourceIterator = source.GetEnumerator())
        {
            if (!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            var min = sourceIterator.Current;
            var minKey = selector(min);
            while (sourceIterator.MoveNext())
            {
                var candidate = sourceIterator.Current;
                var candidateProjected = selector(candidate);
                if (comparer.Compare(candidateProjected, minKey) < 0)
                {
                    min = candidate;
                    minKey = candidateProjected;
                }
            }
            return min;
        }
    }
}
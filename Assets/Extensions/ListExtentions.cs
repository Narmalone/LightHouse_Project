using System.Collections.Generic;
using UnityEngine;

public static class ListExtentions
{
    //variableName.RandomElementInList();
    /// <param name="max"> si -1 prend le count de target </param>
    /// <param name="min"></param>
    /// <returns></returns>
    public static T RandomElementInList<T>(this List<T> target, int max = -1, int min = 0)
    {
        if(max == -1)
        {
            max = target.Count;
        }
        return target[Random.Range(min, max)];
    }

    public static List<T> Shuffle<T>(this List<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
        return ts;
    }

    public static void ClearAndResetObjects<T>(this List<T> target) where T : MonoBehaviour
    {
        for (int i = 0; i < target.Count; i++)
        {
            target[i].enabled = false; // disable the MonoBehaviour
            target[i].gameObject.SetActive(false); // deactivate the GameObject
                                                   // Add any other reset logic you need here
            Object.Destroy(target[i].gameObject);
        }
        target.Clear();
    }
}

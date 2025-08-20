using System.Collections.Generic;
using UnityEngine;

public static class SonarIDAllocator
{
    private static readonly SortedSet<int> _availableIDs = new();
    private static int _nextID = 0;

    public static int AllocateID()
    {
        if (_availableIDs.Count > 0)
        {
            int id = _availableIDs.Min;
            _availableIDs.Remove(id);
            return id;
        }

        return _nextID++;
    }

    public static void ReleaseID(int id)
    {
        _availableIDs.Add(id);
    }

    public static void Reset()
    {
        _availableIDs.Clear();
        _nextID = 0;
    }

    public static string GetDotName(uint id)
    {
        return $"dot{id}";
    }
}

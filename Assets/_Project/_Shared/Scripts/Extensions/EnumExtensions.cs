using System;
using UnityEngine;

namespace LightHouse.Core.Extensions
{
    public static class EnumExtensions
    {
        public static void GetAllFlags<T>(this T argument)
        {
            foreach (T arg in Enum.GetValues(typeof(T)))
            {
                Debug.Log(arg);
            }
        }

        public static bool HasFlag<T>(T argument, T flag) where T : Enum
        {
            return argument.HasFlag(flag);
        }
    }
}

using System;

namespace LightHouse.Locators
{
    public static class Locator<T> where T : class
    {
        private static T _instance;
        public static T Instance => _instance != null
        ? _instance
        : throw new Exception($"Locator<{typeof(T)}> has not been registered!");


        public static event Action<T> OnRegistered;

        public static void Register(T instance)
        {
            _instance = instance;
            OnRegistered?.Invoke(_instance);
        }

        public static void Clear()
        {
            _instance = null;
        }

        public static bool IsInitialized => _instance != null;
    }
}

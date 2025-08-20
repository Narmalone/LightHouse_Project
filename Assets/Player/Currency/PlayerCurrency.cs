using System;

namespace LightHouse.Money
{
    public static class PlayerCurrency
    {
        public static float Balance { get; private set; }

        public static event Action<float> OnBalanceChanged;

        public static void Add(float amount)
        {
            Balance += amount;
            OnBalanceChanged?.Invoke(Balance);
        }

        public static bool TrySpend(float amount)
        {
            if (Balance < amount) return false;
            Balance -= amount;
            OnBalanceChanged?.Invoke(Balance);
            return true;
        }
    }
}

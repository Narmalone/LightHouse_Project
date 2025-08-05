using System;
using UnityEngine;

public static class PlayerCurrency
{
    public static int Balance { get; private set; }

    public static event Action<int> OnBalanceChanged;

    public static void Add(int amount)
    {
        Balance += amount;
        OnBalanceChanged?.Invoke(Balance);
    }

    public static bool TrySpend(int amount)
    {
        if (Balance < amount) return false;
        Balance -= amount;
        OnBalanceChanged?.Invoke(Balance);
        return true;
    }
}

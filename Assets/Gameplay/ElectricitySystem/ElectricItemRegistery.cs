using System;

namespace LightHouse.Electricity
{
    public static class ElectricItemRegistry
    {
        public static event Action<IElectricItem> OnElectricItemRegister;
        public static event Action<IElectricItem> OnElectricItemUnregister;

        public static void Register(IElectricItem item)
        {
            OnElectricItemRegister?.Invoke(item);
        }

        public static void Unregister(IElectricItem item)
        {
            OnElectricItemUnregister?.Invoke(item);
        }
    }

}

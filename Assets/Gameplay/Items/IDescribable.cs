using System;

namespace LightHouse.Interactions
{
    public interface IDescribable
    {
        string GetName();
        string GetDescription();

        //Events
        public event Action OnDescriptionUpdated;
        public event Action OnNameUpdated;
    }

}

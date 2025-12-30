using UnityEngine;


    public interface IOption
    {
        public void Apply();
        public void Revert();
        public bool HasChanges();
   }


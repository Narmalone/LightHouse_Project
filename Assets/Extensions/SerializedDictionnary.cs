using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Collections
{

    [Serializable]
    public class SerializableDictionnaryElement<TKey, TValue>
    {
        public TKey key;
        public TValue value;

        public SerializableDictionnaryElement(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        public List<SerializableDictionnaryElement<TKey, TValue>> elements = new List<SerializableDictionnaryElement<TKey, TValue>>();

        public void OnBeforeSerialize()
        {
            // Vérification et initialisation de la liste si nécessaire
            if (elements == null)
                elements = new List<SerializableDictionnaryElement<TKey, TValue>>();

            elements.Clear();

            foreach (var pair in this)
            {
                elements.Add(new SerializableDictionnaryElement<TKey, TValue>(pair.Key, pair.Value));
            }
        }

        public void OnAfterDeserialize()
        {
            // Vérification et initialisation du dictionnaire
            Clear();

            if (elements == null) return;

            foreach (var element in elements)
            {
                this[element.key] = element.value;
            }
        }
    }
}


using UnityEngine;

namespace LightHouse.Core.Save
{
    public interface ISerializer
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string json);
    }
}

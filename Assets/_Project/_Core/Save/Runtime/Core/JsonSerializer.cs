// JsonUtility ne supporte pas :
// ❌ Dictionary<K,V>
// ❌ Propriétés (seulement fields)
// ❌ Types polymorphiques
// ❌ List<interface>

// Remplace par Newtonsoft (déjà dans Unity via Package Manager) :
using LightHouse.Core.Save;

public class JsonSerializer : ISerializer
{
    private readonly Newtonsoft.Json.JsonSerializerSettings _settings = new()
    {
        Formatting = Newtonsoft.Json.Formatting.Indented,
        Converters = new Newtonsoft.Json.JsonConverter[]
        {
            new Vector3Converter(),
            new QuaternionConverter()
        },
        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto // pour le polymorphisme
    };

    public string Serialize<T>(T obj) =>
        Newtonsoft.Json.JsonConvert.SerializeObject(obj, _settings);

    public T Deserialize<T>(string json) =>
        Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, _settings);
}
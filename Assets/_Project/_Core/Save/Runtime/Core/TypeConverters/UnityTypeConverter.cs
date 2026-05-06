using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LightHouse.Core.Save
{
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, Newtonsoft.Json.JsonSerializer serializer)
        {
            new JObject { ["x"] = value.x, ["y"] = value.y, ["z"] = value.z }
                .WriteTo(writer);
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue,
            bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            return new Vector3(obj["x"].Value<float>(), obj["y"].Value<float>(), obj["z"].Value<float>());
        }
    }

    public class QuaternionConverter : JsonConverter<Quaternion>
    {
        public override void WriteJson(JsonWriter writer, Quaternion value, Newtonsoft.Json.JsonSerializer serializer)
        {
            new JObject { ["x"] = value.x, ["y"] = value.y, ["z"] = value.z, ["w"] = value.w }
                .WriteTo(writer);
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue,
            bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            return new Quaternion(
                obj["x"].Value<float>(), obj["y"].Value<float>(),
                obj["z"].Value<float>(), obj["w"].Value<float>());
        }
    }
}
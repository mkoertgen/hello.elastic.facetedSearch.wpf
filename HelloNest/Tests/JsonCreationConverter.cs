using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HelloNest.Tests
{
    public class JsonCreationConverter<T> : JsonConverter where T : IHaveType, new()
    {
        public override bool CanConvert(Type objectType) { return typeof(T).IsAssignableFrom(objectType); }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            // Create target object based on JObject
            var target = Create(objectType, jObject);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        private static T Create(Type objectType, JObject jObject)
        {
            var typeName = (string)(jObject["type"] ?? objectType.AssemblyQualifiedName);
            var type = Type.GetType(typeName) ?? typeof(T);
            return (T)Activator.CreateInstance(type);
        }


        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
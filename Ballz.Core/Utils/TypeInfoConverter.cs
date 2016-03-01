using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Utils
{
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    // https://stackoverflow.com/questions/19360133/how-to-serialize-object-to-json-with-type-info-using-newtonsoft-json
    public class TypeInfoConverter : JsonConverter
    {
        private readonly IEnumerable<Type> types;

        public TypeInfoConverter()
        {
        }

        public TypeInfoConverter(IEnumerable<Type> types)
        {
            Contract.Requires(types != null);
            this.types = types;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var converters = serializer.Converters.Where(x => !(x is TypeInfoConverter)).ToArray();
            var jObject = new JObject();
            jObject.AddFirst(new JProperty("Type", value.GetType().FullName));
            jObject.Add(new JProperty("Data", JToken.FromObject(value)));
            jObject.WriteTo(writer, converters);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var raw = serializer.Deserialize(reader, objectType);
            Debug.Assert(raw is JObject);
            if (raw == null) return null;
            var jObject = (JObject)raw;
            var type = (string)jObject.GetValue("Type");
            Debug.Assert(type != null);
            var data = jObject.GetValue("Data");
            Debug.Assert(data != null);
            var typeT = Type.GetType(type);
            Debug.Assert(typeT != null);
            if (typeT == null) return null;
            return data.ToObject(typeT);
        }

        public override bool CanConvert(Type objectType)
        {
            return this.types.Any(t => t.IsAssignableFrom(objectType));
        }
    }
}

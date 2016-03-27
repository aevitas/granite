using Newtonsoft.Json;

namespace Granite.Core.Serialization
{
    internal class JsonSerializer : ISerializer
    {
        public static JsonSerializer Instance { get; } =
        new JsonSerializer();

        private JsonSerializer()
        {
            
        }

        public object Deserialize(string serialized)
        {
            if (string.IsNullOrWhiteSpace(serialized))
                return null;

            return JsonConvert.DeserializeObject(serialized);
        }

        public string Serialize(object input)
        {
            if (input == null)
                return string.Empty;

            return JsonConvert.SerializeObject(input);
        }
    }
}

using Newtonsoft.Json;

namespace IfcOperations
{
    public class Item
    {
        [JsonProperty("itemName")]
        public string name;

        [JsonProperty("id")]
        public string id;

        [JsonConstructor]
        public Item(string name, string id)
        {
            this.name = name;
            this.id = id;
        }
    }
}
using Newtonsoft.Json;

namespace SlacrafratMarketDiscordBot.Objects
{
    public struct Item
    {
        [JsonProperty("data")]
        public string data { get; set; }

        [JsonProperty("icon")]
        public string icon { get; set; }

        [JsonProperty("name")]
        public Name name { get; set; }

        public partial class Name
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("key")]
            public string Key { get; set; }

            [JsonProperty("args")]
            public Args Args { get; set; }

            [JsonProperty("lines")]
            public Lines Lines { get; set; }
        }

        public partial class Args
        {
        }

        public partial class Lines
        {
            [JsonProperty("ru")]
            public string Ru { get; set; }

            [JsonProperty("en")]
            public string En { get; set; }
        }
    }
}

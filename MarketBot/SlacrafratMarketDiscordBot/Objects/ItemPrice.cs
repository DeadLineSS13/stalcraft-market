using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlacrafratMarketDiscordBot.Objects
{
    public struct ItemPrice
    {
        [JsonProperty("total")]
        public double Total { get; set; }

        [JsonProperty("prices")]
        public Price[] Prices { get; set; }

        public partial class Price
        {
            [JsonProperty("amount")]
            public long Amount { get; set; }

            [JsonProperty("price")]
            public long PricePrice { get; set; }

            [JsonProperty("time")]
            public DateTimeOffset Time { get; set; }

            [JsonProperty("additional")]
            public Additional Additional { get; set; }
        }

        public partial class Additional
        {
        }
    }
}

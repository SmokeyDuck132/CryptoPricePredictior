using Newtonsoft.Json;
using System.Collections.Generic;

namespace CryptoPricePredictor
{
    public class KlineData
    {
        [JsonProperty("open_time")]
        public long OpenTime { get; set; }

        [JsonProperty("open")]
        public double Open { get; set; }

        [JsonProperty("high")]
        public double High { get; set; }

        [JsonProperty("low")]
        public double Low { get; set; }

        [JsonProperty("close")]
        public double Close { get; set; }

        [JsonProperty("volume")]
        public double Volume { get; set; }

        [JsonProperty("turnover")]
        public double Turnover { get; set; }
    }

    public class BybitKlineResponse
    {
        [JsonProperty("ret_code")]
        public int RetCode { get; set; }

        [JsonProperty("ret_msg")]
        public string RetMsg { get; set; } = string.Empty; // Initialized to prevent CS8618

        [JsonProperty("result")]
        public List<KlineData> Result { get; set; } = new List<KlineData>(); // Initialized to prevent CS8618
    }
}

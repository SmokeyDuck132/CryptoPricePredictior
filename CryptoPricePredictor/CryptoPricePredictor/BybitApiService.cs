using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
// bybit sucks at api, maybe u can use it but i cant so, if u can make it. NICEEE
namespace CryptoPricePredictor
{
    public class BybitApiService
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Fetch spot kline data from Bybit's Spot API.
        /// </summary>
        /// <param name="symbol">Symbol like "BTCUSDT"</param>
        /// <param name="interval">Spot intervals like "1m", "5m", "15m", "1h", "4h", "1D"</param>
        /// <param name="limit">Number of data points to fetch (e.g., 200)</param>
        /// <returns>List of KlineData objects</returns>
        public async Task<List<KlineData>> FetchKlineDataAsync(string symbol, string interval, int limit = 200)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));

            if (string.IsNullOrEmpty(interval))
                throw new ArgumentException("Interval cannot be null or empty.", nameof(interval));

            // Ensure symbol is in correct format (no slash)
            symbol = symbol.Replace("/", "");

            // Spot API endpoint for Kline data
            string url = $"https://api.bybit.com/public/spot/v1/kline?symbol={symbol}&interval={interval}&limit={limit}";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Will throw if not 2xx

            string json = await response.Content.ReadAsStringAsync();
            BybitSpotKlineResponse? apiResponse = JsonConvert.DeserializeObject<BybitSpotKlineResponse>(json);

            if (apiResponse == null || apiResponse.RetCode != 0)
            {
                // If RetCode != 0, there's an API error or invalid symbol/interval
                throw new Exception($"API Error: {apiResponse?.RetMsg ?? "Unknown error"}");
            }

            // Convert SpotKlineData to KlineData objects
            var result = new List<KlineData>();
            foreach (var item in apiResponse.Result)
            {
                // Spot API returns data arrays like:
                // [ open_time(ms), open, high, low, close, volume ]
                // Convert them to KlineData
                var openTimeMs = Convert.ToInt64(item[0]);
                var open = Convert.ToDouble(item[1]);
                var high = Convert.ToDouble(item[2]);
                var low = Convert.ToDouble(item[3]);
                var close = Convert.ToDouble(item[4]);
                var volume = Convert.ToDouble(item[5]);

                result.Add(new KlineData
                {
                    OpenTime = openTimeMs,
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close,
                    Volume = volume,
                    // Spot does not return turnover in the same way as futures,
                    // you can set Turnover = volume or 0.0 if not applicable.
                    Turnover = 0.0
                });
            }

            return result;
        }
    }

    // Classes for the Spot API response format
    public class BybitSpotKlineResponse
    {
        [JsonProperty("ret_code")]
        public int RetCode { get; set; }

        [JsonProperty("ret_msg")]
        public string RetMsg { get; set; } = string.Empty;

        [JsonProperty("result")]
        public List<List<string>> Result { get; set; } = new List<List<string>>();

        [JsonProperty("ext_code")]
        public string ExtCode { get; set; } = string.Empty;

        [JsonProperty("ext_info")]
        public string ExtInfo { get; set; } = string.Empty;

        [JsonProperty("time_now")]
        public string TimeNow { get; set; } = string.Empty;
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CryptoPricePredictor
{
    public class CoinMarketCapApiService
    {
        private readonly HttpClient client;

        public CoinMarketCapApiService()
        {
            client = new HttpClient();
        }

        public void SetApiKey(string apiKey)
        {
            // Clear existing keys just to avoid duplicates
            client.DefaultRequestHeaders.Remove("X-CMC_PRO_API_KEY");
            client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", apiKey);
        }

        /// <summary>
        /// Attempt to fetch historical OHLC data (requires BASIC or higher plan).
        /// If fails (due to insufficient plan), fallback to latest quote.
        /// </summary>
        public async Task<List<KlineData>> FetchHistoricalOhlcAsync(string symbol, string interval, int count = 200)
        {
            // Example endpoint for historical data (BASIC plan required):
            // GET https://pro-api.coinmarketcap.com/v1/cryptocurrency/ohlcv/historical?symbol=BTC&interval=1h&count=200
            string url = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/ohlcv/historical?symbol={symbol}&interval={interval}&count={count}";

            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                // If we get a 400 or 403, maybe FREE plan. Fallback to latest quote.
                return await FetchLatestQuoteAsKline(symbol);
            }

            string json = await response.Content.ReadAsStringAsync();
            var histResponse = JsonConvert.DeserializeObject<CMC_OHLCVHistoricalResponse>(json);
            if (histResponse == null || histResponse.Data == null || histResponse.Data.Quotes == null)
                throw new Exception("Invalid historical OHLC response");

            var result = new List<KlineData>();
            // Quotes array contains the candles
            // Each quote has time_open, time_close, quote.USD fields
            foreach (var q in histResponse.Data.Quotes)
            {
                var openTime = q.TimeOpen;
                var open = q.Quote.USD.Open;
                var high = q.Quote.USD.High;
                var low = q.Quote.USD.Low;
                var close = q.Quote.USD.Close;
                var volume = q.Quote.USD.Volume; // If provided

                result.Add(new KlineData
                {
                    OpenTime = new DateTimeOffset(openTime).ToUnixTimeMilliseconds(),
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close,
                    Volume = volume,
                    Turnover = 0.0
                });
            }

            return result;
        }

        /// <summary>
        /// If historical fails (FREE plan), fallback to latest quote and produce a single "candle".
        /// </summary>
        private async Task<List<KlineData>> FetchLatestQuoteAsKline(string symbol)
        {
            // GET https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol=BTC
            string url = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol={symbol}";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            var quoteResponse = JsonConvert.DeserializeObject<CMC_QuoteLatestResponse>(json);
            if (quoteResponse == null || quoteResponse.Data == null)
                throw new Exception("Invalid latest quote response");

            // Just pick the symbol data, create one "candle" from current price
            // After successful deserialization:
            var data = quoteResponse.Data[symbol.ToUpper()];
            double price = data.Quote["USD"].Price; // Use indexing to access "USD"

            // Create a single "candle" at current time
            var now = DateTime.UtcNow;
            var result = new List<KlineData>
            {
                new KlineData
                {
                    OpenTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Open = price,
                    High = price,
                    Low = price,
                    Close = price,
                    Volume = 0.0,   // Not provided by latest quote
                    Turnover = 0.0
                }
            };

            return result;
        }
    }

    // Models for CMC responses

    public class CMC_OHLCVHistoricalResponse
    {
        [JsonProperty("status")]
        public CMC_Status Status { get; set; }

        [JsonProperty("data")]
        public CMC_OHLCVData Data { get; set; }
    }

    public class CMC_Status
    {
        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }
        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; } = "";
    }

    public class CMC_OHLCVData
    {
        [JsonProperty("quotes")]
        public List<CMC_QuoteCandle> Quotes { get; set; }
    }

    public class CMC_QuoteCandle
    {
        [JsonProperty("time_open")]
        public DateTime TimeOpen { get; set; }

        [JsonProperty("time_close")]
        public DateTime TimeClose { get; set; }

        [JsonProperty("quote")]
        public CMC_QuoteDetail Quote { get; set; }
    }

    public class CMC_QuoteDetail
    {
        [JsonProperty("USD")]
        public CMC_QuoteUSD USD { get; set; }
    }

    public class CMC_QuoteUSD
    {
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
    }

    public class CMC_QuoteLatestResponse
    {
        [JsonProperty("status")]
        public CMC_Status Status { get; set; }

        [JsonProperty("data")]
        public Dictionary<string, CMC_LatestData> Data { get; set; }
    }

    public class CMC_LatestData
    {
        [JsonProperty("quote")]
        public Dictionary<string, CMC_LatestQuoteCurrency> Quote { get; set; }
    }

    public class CMC_LatestQuoteCurrency
    {
        [JsonProperty("price")]
        public double Price { get; set; }
    }
}

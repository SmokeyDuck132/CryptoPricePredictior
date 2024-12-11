## Crypto Price Predictor

**Crypto Price Predictor** is a Windows Forms application written in C# and .NET 8.0 that fetches cryptocurrency data using the [CoinMarketCap API](https://coinmarketcap.com/api/documentation/v1/). It allows users to input their own API key (Free, Basic, or higher subscription levels) and visualize fetched cryptocurrency price data in an interactive chart. The application can compute and display various technical indicators (e.g., SMA, EMA, MACD, Bollinger Bands, RSI) to help users analyze potential price movements.

![Screenshot of Crypto Price Predictor](./screenshot.png)

### Key Features
- **API Key Input**: Users can enter their CoinMarketCap API key directly in the application.
- **Data Fetching**: Retrieves either:
  - Historical OHLC data for a chosen cryptocurrency and interval (for Basic or higher plans).
  - Latest quote (a single data point) for Free plans, allowing a basic view of the current price.
- **Technical Indicators**: SMA, EMA, MACD, Bollinger Bands, and RSI calculations and plotting.
- **Charting Options**: View fetched and analyzed data in the application or open an external chart in the browser.
- **Configurable Intervals & Symbols**: Choose from a list of intervals (e.g., 1h, 4h, 1d) and input a symbol (e.g., BTC).

### Limitations
- **Free Plan**: CoinMarketCap's free tier does not provide historical OHLC data, so you will only get one data point from `quotes/latest`. This is insufficient for meaningful technical indicators.
- **Basic or Higher Plans**: Grants access to the `ohlcv/historical` endpoint, allowing retrieval of multiple candles and full indicator analysis.

---

## Prerequisites

- **.NET 8.0 SDK** or later installed.
- **Visual Studio 2022** Community (or newer) recommended for development.
- A **CoinMarketCap API key**:  
  Sign up at [CoinMarketCap Developer Portal](https://coinmarketcap.com/api/) to get your API key.
  
  - Free plan: Returns a single price point (`quotes/latest`) if `ohlcv/historical` is not accessible.
  - Basic or higher: Allows fetching historical OHLC data, enabling technical indicator calculations.

---

## Installation & Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/yourusername/crypto-price-predictor.git
   cd crypto-price-predictor
   ```

2. **Open the Project**
   - Open the solution (`.sln`) file in **Visual Studio 2022** or newer.

3. **Build the Project**
   - Go to **Build > Build Solution** (or press `Ctrl+Shift+B`).

---

## Configuration

1. **Set Your CoinMarketCap API Key**
   - Run the application.
   - Enter your CoinMarketCap API key in the provided TextBox.
   - Click **"Set API Key"**.  
   
   Once set, the **"Fetch Data"** button becomes enabled.

2. **Select Symbol & Interval**
   - **Symbol**: Enter a symbol like `BTC`.  
     (For historical OHLC, CMC uses simple tickers like `BTC`, `ETH`, etc.)
   - **Time Frame**: Select an interval (`1h`, `4h`, `1d`, etc.) that is supported by your subscription plan.

3. **Fetch & Analyze**
   - Click **"Fetch Data"**:
     - If you have a Basic or higher plan, the app attempts to fetch historical OHLC data.
     - If this fails (Free plan), it falls back to a single latest quote point.
   - Once data is fetched:
     - If you have multiple data points, click **"Analyze & Predict"** to calculate and plot indicators.
     - If on Free plan (only one data point), you’ll see "Not enough data to analyze."

4. **Display Option**
   - **App Chart**: Display fetched data in the integrated chart.
   - **Browser**: If selected, after fetching data, the app can open an external URL related to the symbol.

---

## Usage Tips

- **Insufficient Data**:  
  If you see "Not enough data to analyze," it’s likely because you only received a single data point. Consider upgrading your plan or adjusting intervals.
  
- **Changing Indicators**:  
  The code currently calculates SMA(50), EMA(20), MACD, Bollinger Bands, and RSI(14). You can modify these parameters in the code to require fewer candles (e.g., SMA(5)) or skip some indicators if data is limited.

- **Troubleshooting API Errors**:
  - `HTTP 403`: Your plan does not allow historical OHLC data.
  - `HTTP 404`: Check your symbol or endpoint. For CoinMarketCap, `BTC` is usually a valid symbol.

- **Enhancing Data Visualization**:
  Consider adding tooltips, zooming, and panning features in the chart for a better user experience.

---

## Project Structure

```
crypto-price-predictor/
├─ CryptoPricePredictor.sln
├─ CryptoPricePredictor/
│  ├─ Program.cs
│  ├─ MainForm.cs
│  ├─ CoinMarketCapApiService.cs
│  ├─ TechnicalIndicators.cs
│  ├─ IndicatorAnalyzer.cs
│  ├─ DataExtractor.cs
│  ├─ KlineData.cs
│  ├─ PriceData.cs
│  └─ ... (other UI and resource files)
└─ README.md
```

- **CoinMarketCapApiService.cs**: Handles API calls to CMC endpoints and data parsing.
- **MainForm.cs**: WinForms UI logic, event handlers for buttons, and chart display.
- **TechnicalIndicators.cs**: Calculates SMA, EMA, MACD, RSI, and Bollinger Bands.
- **IndicatorAnalyzer.cs**: Aggregates indicator signals to provide a final prediction.
- **DataExtractor.cs**: Converts raw API data into a structured `PriceData` object.
- **PriceData.cs & KlineData.cs**: Data models representing candle data and transformations.

---

## Contributing

1. Fork the repository.
2. Create a new branch:
   ```bash
   git checkout -b feature/your-feature
   ```
3. Make changes and commit:
   ```bash
   git commit -m "Add your feature description"
   ```
4. Push changes and open a Pull Request on GitHub.

---

## License

This project is licensed under the [MIT License](LICENSE). You are free to use, modify, and distribute this project as you see fit.

---

## Contact

For questions, suggestions, or feedback, feel free to open an issue on the GitHub repository or contact the maintainer at `smokeyduck132@gmail.com`.

---

## Disclaimer

- This tool is for informational and educational purposes only.  
- The displayed indicators and price data do not constitute financial advice.  
- Always perform your own research before making any trading decisions.

---

**Enjoy analyzing crypto price data with the Crypto Price Predictor!**

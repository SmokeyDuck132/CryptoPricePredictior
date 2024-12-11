using Skender.Stock.Indicators;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CryptoPricePredictor
{
    public class MainForm : Form
    {
        // UI Controls
        private Label lblTradingPair;
        private TextBox txtTradingPair;
        private Label lblTimeFrame;
        private ComboBox cmbTimeFrame;
        private GroupBox grpDisplayOption;
        private RadioButton rdoAppChart;
        private RadioButton rdoBrowser;
        private Button btnFetchData;
        private Button btnAnalyzePredict;
        private Chart chartPrice;
        private Label lblPrediction;
        private TextBox txtPrediction;
        private Label lblApiKey;
        private TextBox txtApiKey;
        private Button btnSetApiKey;

        // Services and Data
        private readonly CoinMarketCapApiService apiService = new CoinMarketCapApiService();
        private readonly TechnicalIndicators indicators = new TechnicalIndicators();
        private readonly IndicatorAnalyzer analyzer = new IndicatorAnalyzer();
        private PriceData? currentPriceData; // nullable in case fetch fails
        private bool apiKeySet = false; // Track if user set API key

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Crypto Price Predictor";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            lblApiKey = new Label()
            {
                Text = "API Key:",
                Location = new Point(20, 10),
                AutoSize = true
            };

            txtApiKey = new TextBox()
            {
                Location = new Point(70, 8),
                Width = 300
            };

            btnSetApiKey = new Button()
            {
                Text = "Set API Key",
                Location = new Point(380, 5),
                Size = new Size(100, 30)
            };
            btnSetApiKey.Click += BtnSetApiKey_Click;

            lblTradingPair = new Label()
            {
                Text = "Trading Pair:",
                Location = new Point(20, 50),
                AutoSize = true
            };

            txtTradingPair = new TextBox()
            {
                Text = "BTC", // Just 'BTC' for coin. For CMC, we often use symbol = BTC. 
                // For historical OHLC you use symbol=BTC. It's typically the symbol CMC recognizes.
                Location = new Point(120, 48),
                Width = 100
            };

            lblTimeFrame = new Label()
            {
                Text = "Time Frame:",
                Location = new Point(240, 50),
                AutoSize = true
            };

            cmbTimeFrame = new ComboBox()
            {
                Location = new Point(320, 48),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // Adjust intervals supported by CMC historical if you have Basic plan.
            // Example intervals: '5m', '15m', '30m', '1h', '4h', '1d'
            cmbTimeFrame.Items.AddRange(new string[] { "1h", "4h", "1d" });
            cmbTimeFrame.SelectedIndex = 0;

            grpDisplayOption = new GroupBox()
            {
                Text = "Display Option",
                Location = new Point(440, 40),
                Size = new Size(200, 50)
            };

            rdoAppChart = new RadioButton()
            {
                Text = "App Chart",
                Location = new Point(10, 20),
                Checked = true,
                AutoSize = true
            };

            rdoBrowser = new RadioButton()
            {
                Text = "Browser",
                Location = new Point(100, 20),
                AutoSize = true
            };

            grpDisplayOption.Controls.Add(rdoAppChart);
            grpDisplayOption.Controls.Add(rdoBrowser);

            btnFetchData = new Button()
            {
                Text = "Fetch Data",
                Location = new Point(660, 45),
                Size = new Size(100, 30),
                Enabled = false // Enabled only after API key is set
            };
            btnFetchData.Click += BtnFetchData_Click;

            btnAnalyzePredict = new Button()
            {
                Text = "Analyze & Predict",
                Location = new Point(780, 45),
                Size = new Size(150, 30),
                Enabled = false
            };
            btnAnalyzePredict.Click += BtnAnalyzePredict_Click;

            chartPrice = new Chart()
            {
                Location = new Point(20, 100),
                Size = new Size(1140, 600)
            };
            InitializeChart();

            lblPrediction = new Label()
            {
                Text = "Prediction:",
                Location = new Point(20, 720),
                AutoSize = true
            };

            txtPrediction = new TextBox()
            {
                Location = new Point(100, 718),
                Width = 1060,
                ReadOnly = true
            };

            this.Controls.Add(lblApiKey);
            this.Controls.Add(txtApiKey);
            this.Controls.Add(btnSetApiKey);
            this.Controls.Add(lblTradingPair);
            this.Controls.Add(txtTradingPair);
            this.Controls.Add(lblTimeFrame);
            this.Controls.Add(cmbTimeFrame);
            this.Controls.Add(grpDisplayOption);
            this.Controls.Add(btnFetchData);
            this.Controls.Add(btnAnalyzePredict);
            this.Controls.Add(chartPrice);
            this.Controls.Add(lblPrediction);
            this.Controls.Add(txtPrediction);
        }

        private void InitializeChart()
        {
            chartPrice.ChartAreas.Add(new ChartArea("MainArea"));

            var priceSeries = new Series("Price")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 1,
                Color = Color.Black
            };
            chartPrice.Series.Add(priceSeries);

            var smaSeries = new Series("SMA (50)")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 1,
                Color = Color.Blue
            };
            chartPrice.Series.Add(smaSeries);

            var emaSeries = new Series("EMA (20)")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 1,
                Color = Color.Red
            };
            chartPrice.Series.Add(emaSeries);

            var upperBandSeries = new Series("Bollinger Upper")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 1,
                Color = Color.Green
            };
            chartPrice.Series.Add(upperBandSeries);

            var lowerBandSeries = new Series("Bollinger Lower")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 1,
                Color = Color.Orange
            };
            chartPrice.Series.Add(lowerBandSeries);

            var macdSeries = new Series("MACD")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 1,
                Color = Color.Purple
            };
            chartPrice.Series.Add(macdSeries);

            var macdSignalSeries = new Series("MACD Signal")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 1,
                Color = Color.Gray
            };
            chartPrice.Series.Add(macdSignalSeries);

            chartPrice.ChartAreas["MainArea"].AxisX.Title = "Time";
            chartPrice.ChartAreas["MainArea"].AxisX.LabelStyle.Angle = -45;
            chartPrice.ChartAreas["MainArea"].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chartPrice.ChartAreas["MainArea"].AxisX.LabelStyle.Format = "MM/dd HH:mm";

            chartPrice.ChartAreas["MainArea"].AxisY.Title = "Price";

            chartPrice.Legends.Add(new Legend("Legend"));
        }

        private void BtnSetApiKey_Click(object? sender, EventArgs e)
        {
            var apiKey = txtApiKey.Text.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("Please enter a valid API key.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            apiService.SetApiKey(apiKey);
            apiKeySet = true;
            btnFetchData.Enabled = true;
            txtPrediction.Text = "API key set. You can now fetch data.";
        }

        private async void BtnFetchData_Click(object? sender, EventArgs e)
        {
            if (!apiKeySet)
            {
                MessageBox.Show("Set API key first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string symbol = txtTradingPair.Text.Trim().Replace("/", "");
            string interval = cmbTimeFrame.SelectedItem?.ToString() ?? "1h";
            bool displayInApp = rdoAppChart.Checked;

            try
            {
                btnFetchData.Enabled = false;
                btnAnalyzePredict.Enabled = false;
                txtPrediction.Text = "Fetching data...";

                // Attempt to fetch historical OHLC data (BASIC plan)
                // If fails (FREE plan?), fallback to a single candle from latest quote
                var klineData = await apiService.FetchHistoricalOhlcAsync(symbol, interval, 200);
                currentPriceData = DataExtractor.ExtractPriceData(klineData);

                if (displayInApp)
                {
                    txtPrediction.Text = "Data fetched. Ready for analysis.";
                    btnAnalyzePredict.Enabled = true;
                }
                else
                {
                    DisplayChartInBrowser(symbol);
                    txtPrediction.Text = "Chart opened in browser.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPrediction.Text = "Error fetching data.";
            }
            finally
            {
                btnFetchData.Enabled = true;
            }
        }

        private void DisplayChartInBrowser(string symbol)
        {
            string url = $"https://www.tradingview.com/chart/?symbol={symbol}";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnAnalyzePredict_Click(object? sender, EventArgs e)
        {
            if (currentPriceData == null)
            {
                MessageBox.Show("Please fetch data first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnAnalyzePredict.Enabled = false;
                txtPrediction.Text = "Analyzing indicators...";

                // We can reuse interval for indicators calculation
                string interval = cmbTimeFrame.SelectedItem?.ToString() ?? "1h";

                var sma = indicators.CalculateSMA(currentPriceData.ClosePrices, 50, interval);
                var ema = indicators.CalculateEMA(currentPriceData.ClosePrices, 20, interval);
                var rsi = indicators.CalculateRSI(currentPriceData.ClosePrices, 14, interval);
                var macd = indicators.CalculateMACD(currentPriceData.ClosePrices, interval);
                var bollingerBands = indicators.CalculateBollingerBands(currentPriceData.ClosePrices, 20, 2, interval);

                // Plot the chart
                PlotChart(currentPriceData, sma, ema, rsi, macd, bollingerBands);

                double currentClose = (double)currentPriceData.ClosePrices.Last();
                string prediction = analyzer.AnalyzeIndicators(ema, macd, rsi, bollingerBands, currentClose);
                txtPrediction.Text = prediction;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error analyzing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPrediction.Text = "Error analyzing data.";
            }
            finally
            {
                btnAnalyzePredict.Enabled = true;
            }
        }

        private void PlotChart(PriceData priceData,
                               System.Collections.Generic.List<SmaResult> sma,
                               System.Collections.Generic.List<EmaResult> ema,
                               System.Collections.Generic.List<RsiResult> rsi,
                               System.Collections.Generic.List<MacdResult> macd,
                               System.Collections.Generic.List<BollingerBandsResult> bollingerBands)
        {
            if (priceData.Timestamps.Count == 0)
                return;

            foreach (var series in chartPrice.Series)
            {
                series.Points.Clear();
            }

            // Plot Price
            for (int i = 0; i < priceData.ClosePrices.Count; i++)
            {
                chartPrice.Series["Price"].Points.AddXY(priceData.Timestamps[i], (double)priceData.ClosePrices[i]);
            }

            // Plot SMA
            foreach (var s in sma)
            {
                chartPrice.Series["SMA (50)"].Points.AddXY(s.Date, (double?)s.Sma ?? double.NaN);
            }

            // Plot EMA
            foreach (var e in ema)
            {
                chartPrice.Series["EMA (20)"].Points.AddXY(e.Date, (double?)e.Ema ?? double.NaN);
            }

            // Plot Bollinger Bands
            foreach (var b in bollingerBands)
            {
                chartPrice.Series["Bollinger Upper"].Points.AddXY(b.Date, (double?)b.UpperBand ?? double.NaN);
                chartPrice.Series["Bollinger Lower"].Points.AddXY(b.Date, (double?)b.LowerBand ?? double.NaN);
            }

            // Plot MACD and Signal
            foreach (var m in macd)
            {
                chartPrice.Series["MACD"].Points.AddXY(m.Date, (double?)m.Macd ?? double.NaN);
                chartPrice.Series["MACD Signal"].Points.AddXY(m.Date, (double?)m.Signal ?? double.NaN);
            }

            chartPrice.ChartAreas["MainArea"].RecalculateAxesScale();
        }
    }
}

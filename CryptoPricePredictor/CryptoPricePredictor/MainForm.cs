using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Skender.Stock.Indicators;

namespace CryptoPricePredictor
{
    public partial class MainForm : Form
    {
        private TextBox txtApiKey;
        private TextBox txtApiSecret;
        private Button btnSetApiKey;
        private TextBox txtTradingPair;
        private ComboBox cmbTimeFrame;
        private Button btnFetchData;
        private TextBox txtOutput;
        private BybitApiService? apiService;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Main Form settings
            this.Text = "Crypto Price Predictor";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // API Key Label and TextBox
            var lblApiKey = new Label
            {
                Text = "API Key:",
                Location = new Point(20, 20),
                AutoSize = true
            };
            txtApiKey = new TextBox
            {
                Location = new Point(80, 20),
                Width = 300
            };

            // API Secret Label and TextBox
            var lblApiSecret = new Label
            {
                Text = "API Secret:",
                Location = new Point(20, 60),
                AutoSize = true
            };
            txtApiSecret = new TextBox
            {
                Location = new Point(80, 60),
                Width = 300,
                UseSystemPasswordChar = true
            };

            // Set API Key Button
            btnSetApiKey = new Button
            {
                Text = "Set API Key",
                Location = new Point(400, 20),
                Size = new Size(100, 30)
            };
            btnSetApiKey.Click += BtnSetApiKey_Click;

            // Trading Pair Label and TextBox
            var lblTradingPair = new Label
            {
                Text = "Trading Pair:",
                Location = new Point(20, 100),
                AutoSize = true
            };
            txtTradingPair = new TextBox
            {
                Location = new Point(120, 100),
                Width = 150
            };

            // Time Frame Label and ComboBox
            var lblTimeFrame = new Label
            {
                Text = "Time Frame:",
                Location = new Point(20, 140),
                AutoSize = true
            };
            cmbTimeFrame = new ComboBox
            {
                Location = new Point(120, 140),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTimeFrame.Items.AddRange(new string[] { "1", "5", "15", "60", "240", "D" }); // Valid Bybit intervals
            cmbTimeFrame.SelectedIndex = 3; // Default to 60 minutes (1 hour)

            // Fetch Data Button
            btnFetchData = new Button
            {
                Text = "Fetch Data",
                Location = new Point(300, 100),
                Size = new Size(100, 30)
            };
            btnFetchData.Click += BtnFetchData_Click;

            // Output TextBox
            txtOutput = new TextBox
            {
                Location = new Point(20, 200),
                Width = 1100,
                Height = 500,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };

            // Add controls to the form
            this.Controls.Add(lblApiKey);
            this.Controls.Add(txtApiKey);
            this.Controls.Add(lblApiSecret);
            this.Controls.Add(txtApiSecret);
            this.Controls.Add(btnSetApiKey);
            this.Controls.Add(lblTradingPair);
            this.Controls.Add(txtTradingPair);
            this.Controls.Add(lblTimeFrame);
            this.Controls.Add(cmbTimeFrame);
            this.Controls.Add(btnFetchData);
            this.Controls.Add(txtOutput);
        }

        private void BtnSetApiKey_Click(object sender, EventArgs e)
        {
            var apiKey = txtApiKey.Text.Trim();
            var apiSecret = txtApiSecret.Text.Trim();

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                MessageBox.Show("Please enter both API key and secret.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Initialize the BybitApiService with the provided API key and secret
            apiService = new BybitApiService(apiKey, apiSecret);
            MessageBox.Show("API key and secret set. You can now fetch data.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnFetchData_Click(object sender, EventArgs e)
        {
            if (apiService == null)
            {
                MessageBox.Show("Please set API key and secret first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var symbol = txtTradingPair.Text.Trim();
                var interval = cmbTimeFrame.SelectedItem?.ToString() ?? "60"; // Default to 60 minutes
                var category = "spot"; // Default category (change as needed for derivatives)

                // Fetch Kline data
                var klineData = await apiService.FetchKlineDataAsync(category, symbol, interval, 200);

                // Convert Kline data to Quotes for indicator calculations
                var quotes = klineData.Select(k => new Quote
                {
                    Date = DateTimeOffset.FromUnixTimeMilliseconds(k.OpenTime).UtcDateTime,
                    Open = (decimal)k.Open,
                    High = (decimal)k.High,
                    Low = (decimal)k.Low,
                    Close = (decimal)k.Close,
                    Volume = (decimal)k.Volume
                }).ToList();

                // Calculate Indicators
                var sma = quotes.GetSma(20).ToList();
                var ema = quotes.GetEma(20).ToList();
                var bollingerBands = quotes.GetBollingerBands(20, 2).ToList();
                var macd = quotes.GetMacd(12, 26, 9).ToList();
                var rsi = quotes.GetRsi(14).ToList();

                // Display Data and Indicators
                StringBuilder output = new StringBuilder();
                output.AppendLine("Kline Data with Indicators:");
                foreach (var quote in quotes)
                {
                    var quoteSma = sma.FirstOrDefault(x => x.Date == quote.Date)?.Sma;
                    var quoteEma = ema.FirstOrDefault(x => x.Date == quote.Date)?.Ema;
                    var bb = bollingerBands.FirstOrDefault(x => x.Date == quote.Date);
                    var macdValue = macd.FirstOrDefault(x => x.Date == quote.Date);
                    var rsiValue = rsi.FirstOrDefault(x => x.Date == quote.Date)?.Rsi;

                    output.AppendLine($"Time: {quote.Date}, Open: {quote.Open}, High: {quote.High}, Low: {quote.Low}, Close: {quote.Close}, Volume: {quote.Volume}, " +
                                      $"SMA: {quoteSma}, EMA: {quoteEma}, Bollinger Bands: [{bb?.LowerBand}, {bb?.UpperBand}], MACD: {macdValue?.Macd}, RSI: {rsiValue}");
                }

                txtOutput.Text = output.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

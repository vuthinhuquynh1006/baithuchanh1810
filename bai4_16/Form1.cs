using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bai416
{
    public partial class Form1 : Form
    {
        // 1. Controls Giao diện
        private Button btnStart;
        private Button btnPause;
        private Label lblTitleBTC;
        private Label lblBTCPrice;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblLastUpdate;
        private ToolStripStatusLabel lblSyncCount;
        private DataGridView dgvHistoryLog;

        // 2. Dữ liệu & Threading Control
        private BindingList<TickerModel> historyList;
        private BindingSource bindingSource;
        private CancellationTokenSource cts;
        private decimal previousPrice = 65000.00m; // Giá khởi điểm mẫu
        private int syncCounter = 0;

        public Form1()
        {
            InitializeComponent();
            BuildUI();
            InitializeDataGrid();
        }

        #region Dựng Giao Diện Hoàn Toàn Bằng Code
        private void BuildUI()
        {
            this.Text = "Hệ thống Giám sát Tỷ giá Thời gian thực (frmCryptoStockTicker)";
            this.Size = new Size(900, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // --- GroupBox Điều khiển ---
            GroupBox gbControl = new GroupBox
            {
                Text = "Khu vực điều khiển",
                Location = new Point(12, 12),
                Size = new Size(860, 70)
            };

            btnStart = new Button { Text = "Bắt đầu Giám sát", Location = new Point(20, 25), Size = new Size(130, 30), BackColor = Color.White };
            btnPause = new Button { Text = "Tạm dừng", Location = new Point(165, 25), Size = new Size(110, 30), Enabled = false, BackColor = Color.White };

            btnStart.Click += BtnStart_Click;
            btnPause.Click += BtnPause_Click;

            gbControl.Controls.AddRange(new Control[] { btnStart, btnPause });

            // --- GroupBox Hiển thị Giá Real-time ---
            GroupBox gbDisplay = new GroupBox
            {
                Text = "Tỷ giá BTC/USD Thời gian thực",
                Location = new Point(12, 90),
                Size = new Size(860, 95)
            };

            lblTitleBTC = new Label
            {
                Text = "BTC / USD:",
                Location = new Point(20, 35),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true
            };

            // Đẩy hẳn X = 220 để cách xa chữ "BTC / USD:" rộng rãi, không thể bị chèn đè
            lblBTCPrice = new Label
            {
                Text = "65,000.00",
                Location = new Point(220, 28),
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true
            };

            gbDisplay.Controls.AddRange(new Control[] { lblTitleBTC, lblBTCPrice });

            // --- DataGridView Nhật ký lịch sử ---
            dgvHistoryLog = new DataGridView
            {
                Location = new Point(12, 195),
                Size = new Size(860, 365),
                AutoGenerateColumns = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };

            // --- StatusStrip ---
            statusStrip1 = new StatusStrip();
            lblLastUpdate = new ToolStripStatusLabel("Lần cập nhật cuối: --:--:--  |  ");
            lblSyncCount = new ToolStripStatusLabel("Số lượt đồng bộ thành công: 0");
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblLastUpdate, lblSyncCount });

            this.Controls.AddRange(new Control[] { gbControl, gbDisplay, dgvHistoryLog, statusStrip1 });
        }

        private void InitializeDataGrid()
        {
            dgvHistoryLog.Columns.Clear();
            dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Symbol", HeaderText = "Cặp tiền", Width = 120 });
            dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "Giá (USD)", Width = 180, DefaultCellStyle = { Format = "N2" } });
            dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Trend", HeaderText = "Xu hướng", Width = 140 });
            dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UpdateTime", HeaderText = "Thời gian quét", Width = 220, DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm:ss" } });

            historyList = new BindingList<TickerModel>();
            bindingSource = new BindingSource { DataSource = historyList };
            dgvHistoryLog.DataSource = bindingSource;
        }
        #endregion

        #region Nghiệp vụ Giám sát & Đảm bảo Thread Safety
        private async void BtnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnPause.Enabled = true;

            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            try
            {
                // Vòng lặp ngầm trên Background Thread qua Task.Run
                await Task.Run(async () =>
                {
                    Random rand = new Random();

                    while (!token.IsCancellationRequested)
                    {
                        // Sinh tỷ giá ngẫu nhiên
                        decimal priceChange = (decimal)(rand.NextDouble() * 200 - 98);
                        decimal newPrice = previousPrice + priceChange;
                        string trend = newPrice >= previousPrice ? "Tăng" : "Giảm";
                        previousPrice = newPrice;
                        syncCounter++;

                        var record = new TickerModel
                        {
                            Price = newPrice,
                            Trend = trend,
                            UpdateTime = DateTime.Now
                        };

                        // Cập nhật UI an toàn luồng (Cross-Thread Safety)
                        SafeUpdateUI(record);

                        // Định kỳ 2 giây
                        await Task.Delay(2000, token);
                    }
                }, token);
            }
            catch (TaskCanceledException)
            {
                // Ngắt Task khi Tạm dừng
            }
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
            btnStart.Enabled = true;
            btnPause.Enabled = false;
        }

        /// <summary>
        /// Hàm đảm bảo Thread-Safety với InvokeRequired và this.Invoke
        /// </summary>
        private void SafeUpdateUI(TickerModel record)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<TickerModel>(SafeUpdateUI), new object[] { record });
                return;
            }

            // --- ĐƯỢC THỰC THI THẲNG TRÊN MAIN UI THREAD ---
            lblBTCPrice.Text = $"${record.Price:N2}";
            lblBTCPrice.ForeColor = record.Trend == "Tăng" ? Color.Green : Color.Red;

            historyList.Insert(0, record);

            lblLastUpdate.Text = $"Lần cập nhật cuối: {record.UpdateTime:HH:mm:ss}  |  ";
            lblSyncCount.Text = $"Số lượt đồng bộ thành công: {syncCounter}";
        }
        #endregion
    }
}
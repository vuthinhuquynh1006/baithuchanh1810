using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bai415
{
    public partial class Form1 : Form
    {
        // Controls Tiêu chí
        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private ComboBox cboBranch;
        private Button btnAnalyze;
        private Button btnCancel;

        // Controls Trạng thái
        private ProgressBar pgbLoading;
        private Label lblProcessingStatus;

        // Controls Kết quả
        private DataGridView dgvRevenueData;
        private BindingList<RevenueModel> revenueList;
        private BindingSource bindingSource;

        private CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
            BuildUIWithFlowLayout(); // Sử dụng FlowLayout để tự động xếp ô đẹp mắt, không đè chữ
            InitializeDataGrid();
        }

        private void BuildUIWithFlowLayout()
        {
            this.Text = "Bảng điều khiển Thống kê Doanh thu Multi-Threading (frmRevenueAnalytics)";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(10);

            // 1. GroupBox Tiêu chí
            GroupBox gbFilter = new GroupBox
            {
                Text = "Khu vực tiêu chí thống kê",
                Dock = DockStyle.Top,
                Height = 110,
                Padding = new Padding(10)
            };

            // Dùng FlowLayoutPanel để tự động căn chỉnh khoảng cách các ô
            FlowLayoutPanel flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,
                WrapContents = true
            };

            // Tạo các Label và Control với Margin (khoảng cách) rõ ràng
            Label lblFrom = new Label { Text = "Từ ngày:", AutoSize = true, Margin = new Padding(5, 8, 2, 0) };
            dtpFromDate = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Margin = new Padding(0, 5, 20, 0) };
            dtpFromDate.Value = DateTime.Now.AddDays(-30);

            Label lblTo = new Label { Text = "Đến ngày:", AutoSize = true, Margin = new Padding(5, 8, 2, 0) };
            dtpToDate = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Margin = new Padding(0, 5, 20, 0) };

            Label lblBranch = new Label { Text = "Chi nhánh:", AutoSize = true, Margin = new Padding(5, 8, 2, 0) };
            cboBranch = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 5, 20, 0) };
            cboBranch.Items.AddRange(new string[] { "Tất cả chi nhánh", "Hà Nội", "TP. Hồ Chí Minh", "Đà Nẵng", "Cần Thơ" });
            cboBranch.SelectedIndex = 0;

            btnAnalyze = new Button { Text = "Thống Kê Doanh Thu", Width = 150, Height = 30, BackColor = Color.LightSkyBlue, Margin = new Padding(5, 2, 10, 0) };
            btnCancel = new Button { Text = "Hủy Thao Tác", Width = 110, Height = 30, Enabled = false, Margin = new Padding(0, 2, 0, 0) };

            btnAnalyze.Click += BtnAnalyze_Click;
            btnCancel.Click += BtnCancel_Click;

            // Thêm vào FlowPanel
            flowPanel.Controls.Add(lblFrom);
            flowPanel.Controls.Add(dtpFromDate);
            flowPanel.Controls.Add(lblTo);
            flowPanel.Controls.Add(dtpToDate);
            flowPanel.Controls.Add(lblBranch);
            flowPanel.Controls.Add(cboBranch);
            flowPanel.Controls.Add(btnAnalyze);
            flowPanel.Controls.Add(btnCancel);

            gbFilter.Controls.Add(flowPanel);

            // 2. GroupBox Trạng thái
            GroupBox gbStatus = new GroupBox
            {
                Text = "Khu vực trạng thái xử lý",
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(10)
            };

            pgbLoading = new ProgressBar { Location = new Point(15, 25), Size = new Size(400, 25), Minimum = 0, Maximum = 100, Value = 0 };
            lblProcessingStatus = new Label { Text = "Trạng thái: Sẵn sàng thực hiện", Location = new Point(430, 28), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };

            gbStatus.Controls.Add(pgbLoading);
            gbStatus.Controls.Add(lblProcessingStatus);

            // 3. DataGridView Kết quả
            dgvRevenueData = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };

            // Thêm các thành phần theo thứ tự Dock (Thứ tự thêm rất quan trọng)
            this.Controls.Add(dgvRevenueData);
            this.Controls.Add(gbStatus);
            this.Controls.Add(gbFilter);
        }

        private void InitializeDataGrid()
        {
            dgvRevenueData.Columns.Clear();
            dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TransactionId", HeaderText = "Mã GD", Width = 100 });
            dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TransactionDate", HeaderText = "Ngày giao dịch", Width = 150, DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm" } });
            dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BranchName", HeaderText = "Chi nhánh", Width = 150 });
            dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerName", HeaderText = "Tên khách hàng", Width = 220 });
            dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Amount", HeaderText = "Số tiền (VNĐ)", Width = 160, DefaultCellStyle = { Format = "N0" } });

            revenueList = new BindingList<RevenueModel>();
            bindingSource = new BindingSource { DataSource = revenueList };
            dgvRevenueData.DataSource = bindingSource;
        }

        private async void BtnAnalyze_Click(object sender, EventArgs e)
        {
            btnAnalyze.Enabled = false;
            btnCancel.Enabled = true;
            revenueList.Clear();
            pgbLoading.Value = 0;
            lblProcessingStatus.Text = "Trạng thái: Đang kết nối CSDL và tổng hợp dữ liệu...";
            lblProcessingStatus.ForeColor = Color.Blue;

            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            IProgress<int> progress = new Progress<int>(percent =>
            {
                pgbLoading.Value = percent;
                lblProcessingStatus.Text = $"Trạng thái: Đang xử lý... ({percent}%)";
            });

            string selectedBranch = cboBranch.SelectedItem?.ToString();

            try
            {
                BindingList<RevenueModel> resultData = await Task.Run(() =>
                    FetchAndCalculateRevenueData(selectedBranch, progress, token), token);

                revenueList = resultData;
                bindingSource.DataSource = revenueList;
                lblProcessingStatus.Text = $"Trạng thái: Thống kê hoàn tất! ({revenueList.Count} giao dịch)";
                lblProcessingStatus.ForeColor = Color.Green;
            }
            catch (OperationCanceledException)
            {
                lblProcessingStatus.Text = "Trạng thái: Đã hủy thao tác!";
                lblProcessingStatus.ForeColor = Color.Red;
                pgbLoading.Value = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblProcessingStatus.Text = "Trạng thái: Lỗi xử lý!";
                lblProcessingStatus.ForeColor = Color.Red;
            }
            finally
            {
                btnAnalyze.Enabled = true;
                btnCancel.Enabled = false;
                cts?.Dispose();
                cts = null;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }

        private BindingList<RevenueModel> FetchAndCalculateRevenueData(string branch, IProgress<int> progress, CancellationToken token)
        {
            var list = new BindingList<RevenueModel>();
            Random rand = new Random();
            int totalSteps = 100;

            for (int i = 1; i <= totalSteps; i++)
            {
                token.ThrowIfCancellationRequested();
                Thread.Sleep(50);
                progress?.Report(i);
            }

            int recordCount = rand.Next(15, 30);
            string[] customers = { "Nguyễn Văn Hùng", "Trần Thị Mai", "Lê Hoàng Nam", "Phạm Minh Tuấn", "Đặng Thị Hoa", " Vũ Hoàng Long" };

            for (int i = 1; i <= recordCount; i++)
            {
                list.Add(new RevenueModel
                {
                    TransactionId = $"GD{1000 + i}",
                    TransactionDate = DateTime.Now.AddHours(-rand.Next(1, 720)),
                    BranchName = branch == "Tất cả chi nhánh" ? (i % 2 == 0 ? "Hà Nội" : "TP. Hồ Chí Minh") : branch,
                    CustomerName = customers[rand.Next(customers.Length)],
                    Amount = rand.Next(500, 15000) * 1000
                });
            }

            return list;
        }
    }
}
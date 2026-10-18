using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace bai412
{
    public partial class Form1 : Form
    {
        // Khai báo các Control giao diện
        private TextBox txtMemberId;
        private TextBox txtFullName;
        private TextBox txtPhone;
        private ComboBox cboPackage;
        private DateTimePicker dtpStartDate;
        private TextBox txtFee;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnRefresh;

        private DataGridView dgvMembers;
        private ErrorProvider errorProvider1;

        // Quản lý dữ liệu & Binding
        private BindingList<MemberModel> memberList;
        private BindingSource bindingSource;
        private int currentIdNumber = 1;

        public Form1()
        {
            InitializeComponent();
            BuildUI();
            InitializeCustomComponents();
            LoadInitialData();
        }

        private void BuildUI()
        {
            this.Text = "Hệ thống Quản lý Dịch vụ Phòng Gym & Hội viên";
            this.Size = new Size(820, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            GroupBox gbInfo = new GroupBox
            {
                Text = "Khu vực thông tin hội viên",
                Location = new Point(12, 12),
                Size = new Size(780, 150)
            };

            Label lblMemberId = new Label { Text = "Mã hội viên:", Location = new Point(20, 30), AutoSize = true };
            txtMemberId = new TextBox { Location = new Point(110, 27), Width = 140, ReadOnly = true };

            Label lblFullName = new Label { Text = "Họ và tên:", Location = new Point(20, 68), AutoSize = true };
            txtFullName = new TextBox { Location = new Point(110, 65), Width = 240 };

            Label lblPhone = new Label { Text = "Số điện thoại:", Location = new Point(20, 106), AutoSize = true };
            txtPhone = new TextBox { Location = new Point(110, 103), Width = 240 };

            Label lblPackage = new Label { Text = "Gói tập:", Location = new Point(410, 30), AutoSize = true };
            cboPackage = new ComboBox { Location = new Point(510, 27), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblStartDate = new Label { Text = "Ngày đăng ký:", Location = new Point(410, 68), AutoSize = true };
            dtpStartDate = new DateTimePicker { Location = new Point(510, 65), Width = 220, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };

            Label lblFee = new Label { Text = "Phí dịch vụ:", Location = new Point(410, 106), AutoSize = true };
            txtFee = new TextBox { Location = new Point(510, 103), Width = 220 };

            gbInfo.Controls.AddRange(new Control[] {
                lblMemberId, txtMemberId,
                lblFullName, txtFullName,
                lblPhone, txtPhone,
                lblPackage, cboPackage,
                lblStartDate, dtpStartDate,
                lblFee, txtFee
            });

            GroupBox gbActions = new GroupBox
            {
                Text = "Khu vực chức năng",
                Location = new Point(12, 170),
                Size = new Size(780, 60)
            };

            btnAdd = new Button { Text = "Thêm mới", Location = new Point(110, 20), Size = new Size(110, 30) };
            btnUpdate = new Button { Text = "Sửa thông tin", Location = new Point(250, 20), Size = new Size(110, 30) };
            btnDelete = new Button { Text = "Xóa hội viên", Location = new Point(390, 20), Size = new Size(110, 30) };
            btnRefresh = new Button { Text = "Làm mới", Location = new Point(530, 20), Size = new Size(110, 30) };

            gbActions.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnRefresh });

            dgvMembers = new DataGridView
            {
                Location = new Point(12, 240),
                Size = new Size(780, 280)
            };

            errorProvider1 = new ErrorProvider { ContainerControl = this };

            this.Controls.Add(gbInfo);
            this.Controls.Add(gbActions);
            this.Controls.Add(dgvMembers);
        }

        private void InitializeCustomComponents()
        {
            dgvMembers.AutoGenerateColumns = false;
            dgvMembers.ReadOnly = true;
            dgvMembers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMembers.AllowUserToAddRows = false;

            dgvMembers.Columns.Clear();
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MemberId",
                HeaderText = "Mã HV",
                Width = 90
            });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FullName",
                HeaderText = "Tên hội viên",
                Width = 180
            });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Phone",
                HeaderText = "SĐT",
                Width = 110
            });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Package",
                HeaderText = "Gói tập",
                Width = 110
            });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StartDate",
                HeaderText = "Ngày ĐK",
                Width = 110,
                DefaultCellStyle = { Format = "dd/MM/yyyy" }
            });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Fee",
                HeaderText = "Phí dịch vụ",
                Width = 130,
                DefaultCellStyle = { Format = "N0" }
            });

            memberList = new BindingList<MemberModel>();
            bindingSource = new BindingSource { DataSource = memberList };
            dgvMembers.DataSource = bindingSource;

            txtMemberId.DataBindings.Add("Text", bindingSource, "MemberId", true, DataSourceUpdateMode.Never);
            txtFullName.DataBindings.Add("Text", bindingSource, "FullName", true, DataSourceUpdateMode.OnPropertyChanged);
            txtPhone.DataBindings.Add("Text", bindingSource, "Phone", true, DataSourceUpdateMode.OnPropertyChanged);
            cboPackage.DataBindings.Add("SelectedItem", bindingSource, "Package", true, DataSourceUpdateMode.OnPropertyChanged);
            dtpStartDate.DataBindings.Add("Value", bindingSource, "StartDate", true, DataSourceUpdateMode.OnPropertyChanged);
            txtFee.DataBindings.Add("Text", bindingSource, "Fee", true, DataSourceUpdateMode.OnPropertyChanged, "0", "N0");

            cboPackage.Items.AddRange(new string[] { "Theo tháng", "6 tháng", "1 năm" });
            if (cboPackage.Items.Count > 0) cboPackage.SelectedIndex = 0;

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;

            GenerateNewMemberId();
        }

        private void LoadInitialData()
        {
            memberList.Add(new MemberModel { MemberId = "HV001", FullName = "Nguyễn Văn An", Phone = "0901234567", Package = "Theo tháng", StartDate = DateTime.Now, Fee = 500000 });
            memberList.Add(new MemberModel { MemberId = "HV002", FullName = "Trần Thị Bình", Phone = "0987654321", Package = "6 tháng", StartDate = DateTime.Now.AddDays(-10), Fee = 2700000 });
            currentIdNumber = 3;
        }

        private void GenerateNewMemberId()
        {
            txtMemberId.Text = $"HV{currentIdNumber:D3}";
        }

        private bool ValidateInputs()
        {
            bool isValid = true;
            errorProvider1.Clear();

            string fullName = txtFullName.Text.Trim();
            if (string.IsNullOrEmpty(fullName))
            {
                errorProvider1.SetError(txtFullName, "Họ và tên không được để trống!");
                isValid = false;
            }
            else if (!Regex.IsMatch(fullName, @"^[\p{L}\s]+$"))
            {
                errorProvider1.SetError(txtFullName, "Họ tên không được chứa ký tự đặc biệt hoặc số!");
                isValid = false;
            }

            string phone = txtPhone.Text.Trim();
            if (!Regex.IsMatch(phone, @"^\d{10}$"))
            {
                errorProvider1.SetError(txtPhone, "Số điện thoại phải đúng 10 chữ số!");
                isValid = false;
            }

            if (!decimal.TryParse(txtFee.Text.Trim(), out decimal fee) || fee <= 0)
            {
                errorProvider1.SetError(txtFee, "Phí dịch vụ phải là số nguyên dương!");
                isValid = false;
            }

            return isValid;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            var newMember = new MemberModel
            {
                MemberId = txtMemberId.Text,
                FullName = txtFullName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Package = cboPackage.SelectedItem?.ToString() ?? "Theo tháng",
                StartDate = dtpStartDate.Value,
                Fee = decimal.Parse(txtFee.Text.Trim())
            };

            memberList.Add(newMember);
            currentIdNumber++;
            ClearForm();
            MessageBox.Show("Thêm hội viên mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (bindingSource.Current == null)
            {
                MessageBox.Show("Vui lòng chọn hội viên cần sửa trên lưới!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInputs()) return;

            bindingSource.EndEdit();
            dgvMembers.Refresh();
            MessageBox.Show("Cập nhật thông tin hội viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (bindingSource.Current == null)
            {
                MessageBox.Show("Vui lòng chọn hội viên cần xóa trên lưới!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa hội viên này không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                bindingSource.RemoveCurrent();
                ClearForm();
                MessageBox.Show("Đã xóa hội viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            ClearForm();
            errorProvider1.Clear();
        }

        private void ClearForm()
        {
            GenerateNewMemberId();
            txtFullName.Clear();
            txtPhone.Clear();
            if (cboPackage.Items.Count > 0) cboPackage.SelectedIndex = 0;
            dtpStartDate.Value = DateTime.Now;
            txtFee.Clear();
            dgvMembers.ClearSelection();
            bindingSource.Position = -1;
        }
    }
}
using System;
using System.Drawing;
using System.Windows.Forms;

namespace bai414
{
    public class frmPatientProfiles : Form
    {
        private DataGridView dgvPatients;

        public frmPatientProfiles()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Hồ sơ Bệnh nhân";
            this.Size = new Size(700, 450);

            dgvPatients = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };

            // Dữ liệu mẫu
            dgvPatients.Columns.Add("PatientId", "Mã BN");
            dgvPatients.Columns.Add("FullName", "Họ và tên");
            dgvPatients.Columns.Add("Gender", "Giới tính");
            dgvPatients.Columns.Add("Age", "Tuổi");
            dgvPatients.Columns.Add("Diagnosis", "Chẩn đoán");

            dgvPatients.Rows.Add("BN001", "Nguyễn Văn Hùng", "Nam", 45, "Viêm ruột thừa");
            dgvPatients.Rows.Add("BN002", "Trần Thị Mai", "Nữ", 32, "Sốt xuất huyết");
            dgvPatients.Rows.Add("BN003", "Lê Hoàng Nam", "Nam", 58, "Tăng huyết áp");

            this.Controls.Add(dgvPatients);
        }
    }
}
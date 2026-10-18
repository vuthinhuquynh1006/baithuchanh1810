using System;
using System.Drawing;
using System.Windows.Forms;

namespace bai414
{
    public class frmDoctorSchedule : Form
    {
        private DataGridView dgvDoctors;

        public frmDoctorSchedule()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Lịch trực Bác sĩ";
            this.Size = new Size(700, 450);

            dgvDoctors = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };

            // Dữ liệu mẫu
            dgvDoctors.Columns.Add("DoctorId", "Mã Bác sĩ");
            dgvDoctors.Columns.Add("DoctorName", "Tên Bác sĩ");
            dgvDoctors.Columns.Add("Department", "Khoa/Phòng");
            dgvDoctors.Columns.Add("Shift", "Ca trực");

            dgvDoctors.Rows.Add("BS01", "BS. Phạm Minh Tuấn", "Khoa Cấp cứu", "Ca Đêm");
            dgvDoctors.Rows.Add("BS02", "BS. Lê Thị Hoa", "Khoa Nội", "Ca Sáng");
            dgvDoctors.Rows.Add("BS03", "BS. Đặng Văn Long", "Khoa Ngoại", "Ca Chiều");

            this.Controls.Add(dgvDoctors);
        }
    }
}
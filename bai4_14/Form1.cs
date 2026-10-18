using System;
using System.Drawing;
using System.Windows.Forms;

namespace bai414
{
    public partial class Form1 : Form
    {
        // Thanh hệ thống
        private MenuStrip menuStripMain;
        private StatusStrip statusStripMain;
        private ToolStripStatusLabel lblDoctorStatus;
        private ToolStripProgressBar progressBarStatus;

        public Form1()
        {
            InitializeComponent();
            BuildMdiUI();
        }

        #region Dựng giao diện MDI Container
        private void BuildMdiUI()
        {
            this.Text = "Hệ thống Quản lý Bệnh viện - frmHospitalMain";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 1. Cấu hình Form thành MDI Container
            this.IsMdiContainer = true;

            // 2. Dựng MenuStrip
            menuStripMain = new MenuStrip();

            // Menu Hệ thống
            ToolStripMenuItem menuSystem = new ToolStripMenuItem("Hệ thống");
            ToolStripMenuItem itemLogout = new ToolStripMenuItem("Đăng xuất", null, (s, e) => MessageBox.Show("Đã đăng xuất!", "Thông báo"));
            ToolStripMenuItem itemReload = new ToolStripMenuItem("Nạp lại dữ liệu", null, (s, e) => ReloadData());
            ToolStripMenuItem itemExit = new ToolStripMenuItem("Thoát", null, (s, e) => Application.Exit());
            menuSystem.DropDownItems.AddRange(new ToolStripItem[] { itemLogout, itemReload, new ToolStripSeparator(), itemExit });

            // Menu Quản lý
            ToolStripMenuItem menuManage = new ToolStripMenuItem("Quản lý");
            ToolStripMenuItem itemPatients = new ToolStripMenuItem("Hồ sơ Bệnh nhân", null, (s, e) => OpenMdiChild<frmPatientProfiles>());
            ToolStripMenuItem itemDoctors = new ToolStripMenuItem("Danh mục Bác sĩ (Lịch trực)", null, (s, e) => OpenMdiChild<frmDoctorSchedule>());
            ToolStripMenuItem itemDepartments = new ToolStripMenuItem("Danh mục Khoa/Phòng", null, (s, e) => MessageBox.Show("Chức năng Danh mục Khoa/Phòng đang phát triển!"));
            menuManage.DropDownItems.AddRange(new ToolStripItem[] { itemPatients, itemDoctors, itemDepartments });

            // Menu Cửa sổ (Chức năng sắp xếp MDI Windows)
            ToolStripMenuItem menuWindows = new ToolStripMenuItem("Cửa sổ");
            ToolStripMenuItem itemCascade = new ToolStripMenuItem("Sắp xếp Xếp tầng (Cascade)", null, (s, e) => this.LayoutMdi(MdiLayout.Cascade));
            ToolStripMenuItem itemTileHoriz = new ToolStripMenuItem("Sắp xếp Ngang (Tile Horizontal)", null, (s, e) => this.LayoutMdi(MdiLayout.TileHorizontal));
            ToolStripMenuItem itemTileVert = new ToolStripMenuItem("Sắp xếp Dọc (Tile Vertical)", null, (s, e) => this.LayoutMdi(MdiLayout.TileVertical));
            ToolStripMenuItem itemCloseAll = new ToolStripMenuItem("Đóng tất cả", null, (s, e) => CloseAllMdiChildren());
            menuWindows.DropDownItems.AddRange(new ToolStripItem[] { itemCascade, itemTileHoriz, itemTileVert, new ToolStripSeparator(), itemCloseAll });

            menuStripMain.Items.AddRange(new ToolStripItem[] { menuSystem, menuManage, menuWindows });
            this.MainMenuStrip = menuStripMain;

            // 3. Dựng StatusStrip
            statusStripMain = new StatusStrip();
            lblDoctorStatus = new ToolStripStatusLabel("Bác sĩ trực: BS. Phạm Minh Tuấn | Ca trực: Cấp cứu  |  ");
            progressBarStatus = new ToolStripProgressBar { Value = 100, Width = 150 };

            statusStripMain.Items.AddRange(new ToolStripItem[] { lblDoctorStatus, progressBarStatus });

            // Thêm các thanh điều hướng vào Form Cha
            this.Controls.Add(menuStripMain);
            this.Controls.Add(statusStripMain);
        }
        #endregion

        #region Xử lý Vòng đời & Ngăn mở trùng Form Con MDI
        /// <summary>
        /// Hàm Generic kiểm tra và mở Form con MDI.
        /// Nếu Form đã mở -> Activate() mang lên trên cùng.
        /// Nếu chưa mở -> Khởi tạo mới, gán MdiParent và Show().
        /// </summary>
        private void OpenMdiChild<T>() where T : Form, new()
        {
            // Duyệt qua danh sách các Form con MdiChildren đang mở
            foreach (Form child in this.MdiChildren)
            {
                if (child is T)
                {
                    child.Activate(); // Mang Form đã tồn tại lên trên cùng
                    return;
                }
            }

            // Nếu chưa mở -> tạo Instance mới
            T newChild = new T
            {
                MdiParent = this
            };
            newChild.Show();
        }

        // Đóng toàn bộ Form con
        private void CloseAllMdiChildren()
        {
            foreach (Form child in this.MdiChildren)
            {
                child.Close();
            }
        }

        // Nạp lại dữ liệu
        private void ReloadData()
        {
            progressBarStatus.Value = 0;
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 20 };
            timer.Tick += (s, e) =>
            {
                if (progressBarStatus.Value < 100)
                    progressBarStatus.Value += 10;
                else
                {
                    timer.Stop();
                    MessageBox.Show("Nạp dữ liệu hệ thống thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            timer.Start();
        }
        #endregion
    }
}
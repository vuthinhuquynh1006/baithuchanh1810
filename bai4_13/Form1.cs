using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace bai413
{
    public partial class Form1 : Form
    {
        // Thanh hệ thống
        private MenuStrip menuStrip1;
        private ToolStrip toolStrip1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblTotalBooks;
        private ToolStripStatusLabel lblTotalValue;
        private ToolStripStatusLabel lblCurrentTime;
        private System.Windows.Forms.Timer timerClock;

        // Khu vực chi tiết
        private TextBox txtBookId;
        private TextBox txtTitle;
        private TextBox txtAuthor;
        private TextBox txtPrice;
        private NumericUpDown nudQuantity;
        private PictureBox picBookCover;
        private Button btnBrowseCover;

        // Chức năng
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnSearch;
        private TextBox txtSearch;

        // DataGridView & Binding
        private DataGridView dgvBooks;
        private BindingList<BookModel> bookList;
        private BindingSource bindingSource;

        public Form1()
        {
            InitializeComponent();
            BuildUI();                  // Dựng giao diện hoàn toàn bằng code
            InitializeDataBinding();    // Binding dữ liệu & đồng bộ StatusStrip
            StartClock();               // Bật đồng hồ hiển thị thời gian
        }

        #region 1. Dựng giao diện (Code-behind)
        private void BuildUI()
        {
            this.Text = "Hệ thống Quản lý Sách - frmBookManager";
            this.Size = new Size(950, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // --- 1. MenuStrip ---
            menuStrip1 = new MenuStrip();
            ToolStripMenuItem menuFile = new ToolStripMenuItem("File");
            ToolStripMenuItem itemOpenCSV = new ToolStripMenuItem("Mở file CSV", null, BtnOpenCSV_Click);
            ToolStripMenuItem itemSaveCSV = new ToolStripMenuItem("Lưu file CSV", null, BtnSaveCSV_Click);
            ToolStripMenuItem itemExit = new ToolStripMenuItem("Thoát", null, (s, e) => Application.Exit());
            menuFile.DropDownItems.AddRange(new ToolStripItem[] { itemOpenCSV, itemSaveCSV, new ToolStripSeparator(), itemExit });

            ToolStripMenuItem menuEdit = new ToolStripMenuItem("Edit");
            ToolStripMenuItem itemAdd = new ToolStripMenuItem("Thêm", null, BtnAdd_Click);
            ToolStripMenuItem itemDelete = new ToolStripMenuItem("Xóa", null, BtnDelete_Click);
            ToolStripMenuItem itemSearch = new ToolStripMenuItem("Tìm kiếm", null, (s, e) => txtSearch.Focus());
            menuEdit.DropDownItems.AddRange(new ToolStripItem[] { itemAdd, itemDelete, itemSearch });

            menuStrip1.Items.AddRange(new ToolStripItem[] { menuFile, menuEdit });
            this.MainMenuStrip = menuStrip1;

            // --- 2. ToolStrip ---
            toolStrip1 = new ToolStrip();
            ToolStripButton btnToolOpen = new ToolStripButton("Mở file", null, BtnOpenCSV_Click);
            ToolStripButton btnToolSave = new ToolStripButton("Lưu file", null, BtnSaveCSV_Click);
            ToolStripButton btnToolAdd = new ToolStripButton("Thêm mới", null, BtnAdd_Click);
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnToolOpen, btnToolSave, new ToolStripSeparator(), btnToolAdd });

            // --- 3. StatusStrip ---
            statusStrip1 = new StatusStrip();
            lblTotalBooks = new ToolStripStatusLabel("Tổng số đầu sách: 0  |  ");
            lblTotalValue = new ToolStripStatusLabel("Tổng giá trị tiền sách: 0 VNĐ  |  ");
            lblCurrentTime = new ToolStripStatusLabel("Thời gian: " + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy"));
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblTotalBooks, lblTotalValue, lblCurrentTime });

            // --- 4. GroupBox Chi tiết ---
            GroupBox gbDetail = new GroupBox
            {
                Text = "Khu vực chi tiết sách",
                Location = new Point(12, 60),
                Size = new Size(910, 180)
            };

            Label lblId = new Label { Text = "Mã sách:", Location = new Point(20, 30), AutoSize = true };
            txtBookId = new TextBox { Location = new Point(100, 27), Width = 150 };

            Label lblTitle = new Label { Text = "Tên sách:", Location = new Point(20, 65), AutoSize = true };
            txtTitle = new TextBox { Location = new Point(100, 62), Width = 280 };

            Label lblAuthor = new Label { Text = "Tác giả:", Location = new Point(20, 100), AutoSize = true };
            txtAuthor = new TextBox { Location = new Point(100, 97), Width = 280 };

            Label lblPrice = new Label { Text = "Giá tiền:", Location = new Point(20, 135), AutoSize = true };
            txtPrice = new TextBox { Location = new Point(100, 132), Width = 140 };

            Label lblQty = new Label { Text = "Số lượng:", Location = new Point(260, 135), AutoSize = true };
            nudQuantity = new NumericUpDown { Location = new Point(320, 132), Width = 60, Minimum = 1, Maximum = 10000, Value = 1 };

            // PictureBox
            picBookCover = new PictureBox
            {
                Location = new Point(420, 25),
                Size = new Size(110, 140),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            btnBrowseCover = new Button
            {
                Text = "Tải ảnh bìa",
                Location = new Point(540, 25),
                Size = new Size(100, 30)
            };
            btnBrowseCover.Click += BtnBrowseCover_Click;

            // Tìm kiếm & Nút bấm
            Label lblSearch = new Label { Text = "Tìm tên sách:", Location = new Point(660, 28), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(660, 50), Width = 230 };
            btnSearch = new Button { Text = "Tìm kiếm", Location = new Point(660, 80), Size = new Size(80, 30) };
            btnSearch.Click += BtnSearch_Click;

            btnAdd = new Button { Text = "Thêm mới", Location = new Point(540, 132), Size = new Size(85, 30) };
            btnUpdate = new Button { Text = "Sửa", Location = new Point(635, 132), Size = new Size(75, 30) };
            btnDelete = new Button { Text = "Xóa", Location = new Point(720, 132), Size = new Size(75, 30) };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;

            gbDetail.Controls.AddRange(new Control[] {
                lblId, txtBookId,
                lblTitle, txtTitle,
                lblAuthor, txtAuthor,
                lblPrice, txtPrice,
                lblQty, nudQuantity,
                picBookCover, btnBrowseCover,
                lblSearch, txtSearch, btnSearch,
                btnAdd, btnUpdate, btnDelete
            });

            // --- 5. DataGridView ---
            dgvBooks = new DataGridView
            {
                Location = new Point(12, 250),
                Size = new Size(910, 350),
                AutoGenerateColumns = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };

            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BookId", HeaderText = "Mã sách", Width = 90 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tên sách", Width = 230 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Author", HeaderText = "Tác giả", Width = 150 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "Đơn giá", Width = 110, DefaultCellStyle = { Format = "N0" } });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Số lượng", Width = 80 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CoverImagePath", HeaderText = "Đường dẫn ảnh", Width = 200 });

            this.Controls.Add(dgvBooks);
            this.Controls.Add(gbDetail);
            this.Controls.Add(toolStrip1);
            this.Controls.Add(menuStrip1);
            this.Controls.Add(statusStrip1);
        }
        #endregion

        #region 2. DataBinding & StatusStrip
        private void InitializeDataBinding()
        {
            bookList = new BindingList<BookModel>();
            bookList.ListChanged += (s, e) => UpdateStatusStrip();

            bindingSource = new BindingSource { DataSource = bookList };
            dgvBooks.DataSource = bindingSource;

            txtBookId.DataBindings.Add("Text", bindingSource, "BookId", true, DataSourceUpdateMode.Never);
            txtTitle.DataBindings.Add("Text", bindingSource, "Title", true, DataSourceUpdateMode.OnPropertyChanged);
            txtAuthor.DataBindings.Add("Text", bindingSource, "Author", true, DataSourceUpdateMode.OnPropertyChanged);
            txtPrice.DataBindings.Add("Text", bindingSource, "Price", true, DataSourceUpdateMode.OnPropertyChanged, "0", "N0");
            nudQuantity.DataBindings.Add("Value", bindingSource, "Quantity", true, DataSourceUpdateMode.OnPropertyChanged);

            bindingSource.CurrentChanged += (s, e) => ShowCurrentImage();

            // Dữ liệu mẫu
            bookList.Add(new BookModel { BookId = "B001", Title = "Lập trình C# WinForms", Author = "Nguyễn Văn A", Price = 120000, Quantity = 10, CoverImagePath = "" });
            bookList.Add(new BookModel { BookId = "B002", Title = "Cấu trúc dữ liệu & Giải thuật", Author = "Trần Thị B", Price = 150000, Quantity = 5, CoverImagePath = "" });

            UpdateStatusStrip();
        }

        private void UpdateStatusStrip()
        {
            int totalBooks = bookList.Count;
            decimal totalValue = bookList.Sum(b => b.TotalValue);

            lblTotalBooks.Text = $"Tổng số đầu sách: {totalBooks}  |  ";
            lblTotalValue.Text = $"Tổng giá trị kho: {totalValue:N0} VNĐ  |  ";
        }

        private void StartClock()
        {
            timerClock = new System.Windows.Forms.Timer { Interval = 1000 };
            timerClock.Tick += (s, e) =>
            {
                lblCurrentTime.Text = "Thời gian: " + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            };
            timerClock.Start();
        }

        private void ShowCurrentImage()
        {
            if (bindingSource.Current is BookModel book && !string.IsNullOrEmpty(book.CoverImagePath) && File.Exists(book.CoverImagePath))
            {
                picBookCover.ImageLocation = book.CoverImagePath;
            }
            else
            {
                picBookCover.Image = null;
            }
        }
        #endregion

        #region 3. Nghiệp vụ Quản lý & Ảnh
        private void BtnBrowseCover_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.png; *.jpg; *.jpeg)|*.png;*.jpg;*.jpeg";
                ofd.Title = "Chọn ảnh bìa sách";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    picBookCover.ImageLocation = ofd.FileName;
                    if (bindingSource.Current is BookModel currentBook)
                    {
                        currentBook.CoverImagePath = ofd.FileName;
                        bindingSource.ResetCurrentItem();
                    }
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBookId.Text) || string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Mã sách và Tên sách không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price) || price < 0)
            {
                MessageBox.Show("Đơn giá phải là số hợp lệ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newBook = new BookModel
            {
                BookId = txtBookId.Text.Trim(),
                Title = txtTitle.Text.Trim(),
                Author = txtAuthor.Text.Trim(),
                Price = price,
                Quantity = (int)nudQuantity.Value,
                CoverImagePath = picBookCover.ImageLocation ?? ""
            };

            bookList.Add(newBook);
            MessageBox.Show("Thêm sách thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (bindingSource.Current == null) return;

            bindingSource.EndEdit();
            dgvBooks.Refresh();
            UpdateStatusStrip();
            MessageBox.Show("Cập nhật thông tin sách thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (bindingSource.Current == null) return;

            if (MessageBox.Show("Bạn có chắc muốn xóa sách này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                bindingSource.RemoveCurrent();
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(keyword))
            {
                dgvBooks.DataSource = bindingSource;
            }
            else
            {
                var filtered = bookList.Where(b => b.Title.ToLower().Contains(keyword) || b.Author.ToLower().Contains(keyword)).ToList();
                dgvBooks.DataSource = filtered;
            }
        }
        #endregion

        #region 4. Nhập / Xuất dữ liệu CSV
        private void BtnSaveCSV_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                sfd.FileName = "DanhSachSach.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("BookId,Title,Author,Price,Quantity,CoverImagePath");

                        foreach (var book in bookList)
                        {
                            sb.AppendLine($"{book.BookId},{book.Title},{book.Author},{book.Price},{book.Quantity},{book.CoverImagePath}");
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi lưu file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnOpenCSV_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV Files (*.csv)|*.csv";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                        bookList.Clear();

                        for (int i = 1; i < lines.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(lines[i])) continue;

                            string[] parts = lines[i].Split(',');
                            if (parts.Length >= 6)
                            {
                                bookList.Add(new BookModel
                                {
                                    BookId = parts[0],
                                    Title = parts[1],
                                    Author = parts[2],
                                    Price = decimal.TryParse(parts[3], out decimal p) ? p : 0,
                                    Quantity = int.TryParse(parts[4], out int q) ? q : 0,
                                    CoverImagePath = parts[5]
                                });
                            }
                        }

                        MessageBox.Show("Nhập dữ liệu từ CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi mở file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion
    }
}
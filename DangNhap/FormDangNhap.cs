using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.DangNhap
{
    public partial class FormDangNhap : Form
    {
        // Thông tin phiên đăng nhập – các form khác đọc trực tiếp
        public static string MaNV_DangNhap = string.Empty;
        public static string TenNV_DangNhap = string.Empty;
        public static int CapDoQuyen_DangNhap;

        public FormDangNhap()
        {
            InitializeComponent();
            ActiveControl = txtTenDangNhap;
        }

        private async void btnDangNhap_Click(object sender, EventArgs e)
        {
            string tenDangNhap = txtTenDangNhap.Text.Trim();
            string matKhau = txtMatKhau.Text;
            if (tenDangNhap.Length == 0 || matKhau.Length == 0)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            btnDangNhap.Enabled = false;
            try
            {
                bool hopLe = await KiemTraDangNhapAsync(tenDangNhap, matKhau);
                if (hopLe)
                {
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    txtMatKhau.Clear();
                    txtMatKhau.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message + "\n\nChi tiết: " + (ex.InnerException?.Message ?? ""), "Lỗi kết nối",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDangNhap.Enabled = true;
            }
        }

        private async Task<bool> KiemTraDangNhapAsync(string tenDangNhap, string matKhau)
        {
            await using var db = new HotelManagementContext();

            // Kiểm tra kết nối trước
            bool canConnect = await db.Database.CanConnectAsync();
            if (!canConnect)
            {
                MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu SQL Server!\nHãy đảm bảo SQL Server đang chạy và tên Server trong file 'Hotelmanagementcontext.custom.cs' đã đúng.", "Lỗi kết nối CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var tk = await db.TaiKhoans
                .AsNoTracking()
                .Where(t => t.TenTk == tenDangNhap && t.DaXoa != true)
                .Select(t => new
                {
                    t.Password,
                    t.MaNv,
                    t.CapDoQuyen,
                    TenNv = t.MaNvNavigation.TenNv,
                    NhanVienDaXoa = t.MaNvNavigation.DaXoa
                })
                .FirstOrDefaultAsync();

            if (tk == null)
            {
                MessageBox.Show($"Không tìm thấy tài khoản '{tenDangNhap}' trong CSDL HotelManagement!", "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (tk.NhanVienDaXoa == true)
            {
                MessageBox.Show("Tài khoản của nhân viên này đã bị xóa hoặc khóa!", "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!string.Equals(tk.Password.Trim(), matKhau.Trim(), StringComparison.Ordinal))
            {
                MessageBox.Show($"Mật khẩu không đúng! (Mật khẩu nhập: '{matKhau}')", "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            MaNV_DangNhap = tk.MaNv;
            TenNV_DangNhap = tk.TenNv;
            CapDoQuyen_DangNhap = tk.CapDoQuyen;
            Tag = tk.TenNv;
            return true;
        }

        private void txtMatKhau_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnDangNhap.PerformClick();
            }
        }
    }
}

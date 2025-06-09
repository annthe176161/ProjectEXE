using ProjectEXE.ViewModel.Voucher;

namespace ProjectEXE.Services.Interfaces
{
    public interface IVourcherService
    {
        // Lấy danh sách tất cả voucher
        Task<List<VoucherViewModel>> GetAllVouchers();

        // Lấy voucher theo ID
        Task<VoucherViewModel> GetVoucherById(string id);

        // Tạo mới voucher
        Task<bool> CreateVoucher(VoucherViewModel model);

        // Cập nhật voucher
        Task<bool> UpdateVoucher(VoucherViewModel model);

        // Xóa voucher
        Task<bool> DeleteVoucher(string id);

        // Kiểm tra mã voucher đã tồn tại chưa
        Task<bool> IsCodeUnique(string code, string currentId = null);
    }
}

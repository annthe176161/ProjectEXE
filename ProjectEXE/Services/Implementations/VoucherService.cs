using Microsoft.EntityFrameworkCore;
using ProjectEXE.Controllers;
using ProjectEXE.DTO;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.Voucher;

namespace ProjectEXE.Services.Implementations
{
    public class VoucherService : IVourcherService
    {

        public readonly RevaContext _context;
        public VoucherService(RevaContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateVoucher(VoucherViewModel model)
        {
            try
            {
                // Tạo entity mới từ viewmodel
                var voucher = new Voucher
                {
                    // Tạo ID mới là một chuỗi GUID
                    VourcherId = Guid.NewGuid().ToString(),
                    Code = model.Code,
                    Discount = model.Discount,
                    // Ngày tạo là ngày hiện tại
                    CreateAt = DateOnly.FromDateTime(DateTime.Now),
                    ExpiryDate = model.ExpiryDate,
                    MinOrderValue = model.MinOrderValue,
                    MaxDiscountAmount = model.MaxDiscountAmount,
                    Quantity = model.Quantity,
                    IsActive = model.IsActive
                };

                // Thêm vào DbContext
                await _context.Vouchers.AddAsync(voucher);

                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteVoucher(string id)
        {
            try
            {
                // Tìm voucher cần xóa
                var voucher = await _context.Vouchers.FindAsync(id);

                if (voucher == null)
                    return false;

                // Xóa khỏi DbContext
                _context.Vouchers.Remove(voucher);

                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<VoucherViewModel>> GetAllVouchers()
        {
            // Lấy danh sách voucher từ database
            var vouchers = await _context.Vouchers.ToListAsync();

            // Chuyển từ entity sang viewmodel
            var result = new List<VoucherViewModel>();
            foreach (var item in vouchers)
            {
                result.Add(new VoucherViewModel
                {
                    VoucherId = item.VourcherId,
                    Code = item.Code,
                    Discount = item.Discount,
                    CreateAt = item.CreateAt,
                    ExpiryDate = item.ExpiryDate,
                    MinOrderValue = item.MinOrderValue,
                    MaxDiscountAmount = item.MaxDiscountAmount,
                    Quantity = item.Quantity,
                    IsActive = item.IsActive
                });
            }

            return result;
        }

        public async Task<VoucherViewModel> GetVoucherById(string id)
        {
            // Tìm voucher theo ID
            var voucher = await _context.Vouchers.FindAsync(id);

            if (voucher == null)
                return null;

            // Chuyển từ entity sang viewmodel
            return new VoucherViewModel
            {
                VoucherId = voucher.VourcherId,
                Code = voucher.Code,
                Discount = voucher.Discount,
                CreateAt = voucher.CreateAt,
                ExpiryDate = voucher.ExpiryDate,
                MinOrderValue = voucher.MinOrderValue,
                MaxDiscountAmount = voucher.MaxDiscountAmount,
                Quantity = voucher.Quantity,
                IsActive = voucher.IsActive
            };
        }


        // Kiểm tra xem mã voucher đã tồn tại chưa
        public async Task<bool> IsCodeUnique(string code, string currentId = null)
        {
            // Nếu đang tạo mới (không có currentId)
            if (string.IsNullOrEmpty(currentId))
            {
                // Kiểm tra xem có voucher nào có cùng mã không
                return !await _context.Vouchers.AnyAsync(v => v.Code == code);
            }
            // Nếu đang cập nhật
            else
            {
                // Kiểm tra xem có voucher nào khác có cùng mã không
                return !await _context.Vouchers.AnyAsync(v => v.Code == code && v.VourcherId != currentId);
            }
        }

        public async Task<bool> UpdateVoucher(VoucherViewModel model)
        {
            try
            {
                // Tìm voucher cần cập nhật
                var voucher = await _context.Vouchers.FindAsync(model.VoucherId);

                if (voucher == null)
                    return false;

                // Cập nhật thông tin
                voucher.Code = model.Code;
                voucher.Discount = model.Discount;
                voucher.ExpiryDate = model.ExpiryDate;
                voucher.MinOrderValue = model.MinOrderValue;
                voucher.MaxDiscountAmount = model.MaxDiscountAmount;
                voucher.Quantity = model.Quantity;
                voucher.IsActive = model.IsActive;

                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> AddVoucherAtRegister(int discount)
        {
            var code = $"INV-{ReferralCodeGenerator.Generate(10)}";
            var voucher = new Voucher
            {
                VourcherId = Guid.NewGuid().ToString(),
                Code = code,
                Discount = discount,
                CreateAt = DateOnly.FromDateTime(DateTime.Now),
                MinOrderValue = 0,
                MaxDiscountAmount = 20000,
                Quantity = 1,
                IsActive = true
            };

            await  _context.Vouchers.AddAsync(voucher);
            await _context.SaveChangesAsync();

            return code;
        }
    }
}

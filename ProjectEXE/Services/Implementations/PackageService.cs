using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.ServicePackageViewModel;

namespace ProjectEXE.Services.Implementations
{
    public class PackageService : IPackageService
    {
        private readonly RevaContext _context;
        private readonly IConfiguration _configuration;

        public PackageService(RevaContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<List<ServicePackage>> GetAllPackagesAsync()
        {
            return await _context.ServicePackages.ToListAsync();
        }

        public async Task<ServicePackage> GetPackageByIdAsync(int packageId)
        {
            return await _context.ServicePackages.FindAsync(packageId);
        }

        public async Task<ServicePackageListViewModel> GetPackagesForShopAsync(int shopId)
        {
            var packages = await GetAllPackagesAsync();

            // Kiểm tra gói hiện tại của shop
            var currentSubscription = await _context.PackageSubscriptions
                .Where(ps => ps.ShopId == shopId && ps.StatusId == 1) // StatusId 1 = Active
                .OrderByDescending(ps => ps.EndDate)
                .FirstOrDefaultAsync();

            return new ServicePackageListViewModel
            {
                Packages = packages,
                CurrentPackageId = currentSubscription?.PackageId,
                CurrentSubscriptionEndDate = currentSubscription?.EndDate
            };
        }

        public async Task<PaymentInformationViewModel> CreatePaymentRequestAsync(int packageId, int shopId, int userId)
        {
            var package = await GetPackageByIdAsync(packageId);
            if (package == null)
            {
                throw new Exception("Gói dịch vụ không tồn tại");
            }

            // Tạo mã giao dịch
            string transactionCode = GenerateTransactionCode();

            // Tạo đối tượng thanh toán
            var payment = new PackagePayment
            {
                UserId = userId,
                PackageId = packageId,
                TransactionCode = transactionCode,
                Amount = package.DiscountedPrice ?? package.Price, // Sử dụng giá khuyến mãi nếu có
                StatusId = 1, // Pending
                CreatedAt = DateTime.Now
            };

            _context.PackagePayments.Add(payment);
            await _context.SaveChangesAsync();

            // Lấy thông tin admin từ appsettings
            string adminBankAccount = _configuration["AdminPayment:BankAccount"];
            string adminBankName = _configuration["AdminPayment:BankName"];
            string adminQRCode = _configuration["AdminPayment:QRCodeUrl"];

            // Tạo nội dung chuyển khoản
            string paymentInstruction = $"REVA {transactionCode}";

            return new PaymentInformationViewModel
            {
                PackageId = packageId,
                PackageName = package.PackageName,
                Amount = payment.Amount,
                TransactionCode = transactionCode,
                AdminQRCode = adminQRCode,
                PaymentInstruction = paymentInstruction,
                AdminBankAccount = adminBankAccount,
                AdminBankName = adminBankName,
                ShopId = shopId
            };
        }

        public async Task<PaymentCompletionViewModel> ConfirmPaymentAsync(string transactionCode, int userId)
        {
            var payment = await _context.PackagePayments
                .Include(p => p.Package)
                .FirstOrDefaultAsync(p => p.TransactionCode == transactionCode && p.UserId == userId);

            if (payment == null)
            {
                throw new Exception("Không tìm thấy thông tin thanh toán");
            }

            // Cập nhật trạng thái thanh toán
            payment.StatusId = 1; // Pending (chờ admin xác nhận)
            await _context.SaveChangesAsync();

            return new PaymentCompletionViewModel
            {
                PaymentId = payment.PaymentId,
                TransactionCode = payment.TransactionCode,
                Amount = payment.Amount,
                PackageName = payment.Package.PackageName,
                PaymentDate = DateTime.Now
            };
        }

        public async Task<bool> HasActiveSubscriptionAsync(int shopId)
        {
            return await _context.PackageSubscriptions
                .AnyAsync(ps => ps.ShopId == shopId &&
                           ps.StatusId == 1 && // Active
                           ps.EndDate > DateTime.Now);
        }

        public async Task<DateTime?> GetSubscriptionEndDateAsync(int shopId)
        {
            var subscription = await _context.PackageSubscriptions
                .Where(ps => ps.ShopId == shopId && ps.StatusId == 1) // Active
                .OrderByDescending(ps => ps.EndDate)
                .FirstOrDefaultAsync();

            return subscription?.EndDate;
        }

        private string GenerateTransactionCode()
        {
            // Tạo mã giao dịch ngẫu nhiên gồm 8 ký tự
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code = new string(Enumerable.Repeat(characters, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Thêm thời gian để đảm bảo duy nhất
            string timeStamp = DateTime.Now.ToString("MMddHHmm");
            return $"{code}{timeStamp}";
        }
    }
}

using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.Admin;

namespace ProjectEXE.Services.Implementations
{
    public class AdminPackageService : IAdminPackageService
    {
        private readonly RevaContext _context;

        public AdminPackageService(RevaContext context)
        {
            _context = context;
        }

        public async Task<PackagePaymentListViewModel> GetPaymentDashboardAsync()
        {
            // Lấy thanh toán đang chờ xử lý
            var pendingPayments = await GetPaymentsQuery(1)
                .Take(10)
                .ToListAsync();

            // Lấy thanh toán gần đây đã xử lý
            var recentPayments = await _context.PackagePayments
                .Where(p => p.StatusId == 2 || p.StatusId == 3) // Confirmed or Rejected
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Join(_context.Users, p => p.UserId, u => u.UserId, (p, u) => new { Payment = p, User = u })
                .Join(_context.ServicePackages, pu => pu.Payment.PackageId, pk => pk.PackageId, (pu, pk) => new { pu.Payment, pu.User, Package = pk })
                .Join(_context.PaymentStatuses, pup => pup.Payment.StatusId, s => s.StatusId, (pup, s) => new { pup.Payment, pup.User, pup.Package, Status = s })
                .Select(x => MapToViewModel(x.Payment, x.User, x.Package, x.Status))
                .ToListAsync();

            // Lấy thống kê
            var totalPending = await _context.PackagePayments.CountAsync(p => p.StatusId == 1);
            var totalConfirmed = await _context.PackagePayments.CountAsync(p => p.StatusId == 2);
            var totalRejected = await _context.PackagePayments.CountAsync(p => p.StatusId == 3);
            var totalRevenue = await _context.PackagePayments.Where(p => p.StatusId == 2).SumAsync(p => p.Amount);

            return new PackagePaymentListViewModel
            {
                PendingPayments = pendingPayments,
                RecentPayments = recentPayments,
                TotalPending = totalPending,
                TotalConfirmed = totalConfirmed,
                TotalRejected = totalRejected,
                TotalRevenue = totalRevenue
            };
        }

        public async Task<PackagePaymentViewModel> GetPaymentDetailsAsync(int paymentId)
        {
            var payment = await _context.PackagePayments
                .Where(p => p.PaymentId == paymentId)
                .Join(_context.Users, p => p.UserId, u => u.UserId, (p, u) => new { Payment = p, User = u })
                .Join(_context.ServicePackages, pu => pu.Payment.PackageId, pk => pk.PackageId, (pu, pk) => new { pu.Payment, pu.User, Package = pk })
                .Join(_context.PaymentStatuses, pup => pup.Payment.StatusId, s => s.StatusId, (pup, s) => new { pup.Payment, pup.User, pup.Package, Status = s })
                .Select(x => new { x.Payment, x.User, x.Package, x.Status })
                .FirstOrDefaultAsync();

            if (payment == null)
                return null;

            // Tìm ShopId của User
            var shop = await _context.Shops
                .Where(s => s.UserId == payment.User.UserId)
                .FirstOrDefaultAsync();

            var result = MapToViewModel(payment.Payment, payment.User, payment.Package, payment.Status);

            if (shop != null)
            {
                result.ShopId = shop.ShopId;
                result.ShopName = shop.ShopName;
            }

            return result;
        }

        public async Task<List<PackagePaymentViewModel>> GetPaymentsByStatusAsync(int statusId, int page = 1, int pageSize = 10)
        {
            return await GetPaymentsQuery(statusId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> ConfirmPaymentAsync(int paymentId, int adminUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Lấy thông tin thanh toán
                var payment = await _context.PackagePayments
                    .Include(p => p.Package)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (payment == null || payment.StatusId != 1) // Không tồn tại hoặc không phải đang chờ
                    return false;

                // Lấy shopId của người dùng
                var shop = await _context.Shops
                    .FirstOrDefaultAsync(s => s.UserId == payment.UserId);

                if (shop == null)
                    return false;

                // Cập nhật trạng thái thanh toán
                payment.StatusId = 2; // Confirmed

                // Kiểm tra xem đã có gói đang hoạt động không
                var activeSubscription = await _context.PackageSubscriptions
                    .Where(ps => ps.ShopId == shop.ShopId && ps.StatusId == 1 && ps.EndDate > DateTime.Now)
                    .OrderByDescending(ps => ps.EndDate)
                    .FirstOrDefaultAsync();

                DateTime startDate = DateTime.Now;
                if (activeSubscription != null)
                {
                    // Nếu đã có gói hoạt động, thì gói mới sẽ bắt đầu sau khi gói cũ hết hạn
                    startDate = activeSubscription.EndDate;

                    // Nếu cùng một gói, tăng thời hạn lên
                    if (activeSubscription.PackageId == payment.PackageId)
                    {
                        activeSubscription.EndDate = activeSubscription.EndDate.AddDays(30);
                        _context.PackageSubscriptions.Update(activeSubscription);

                        // Liên kết thanh toán với gói đã có
                        payment.SubscriptionId = activeSubscription.SubscriptionId;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
                }

                // Tạo gói đăng ký mới
                var subscription = new PackageSubscription
                {
                    ShopId = shop.ShopId,
                    PackageId = payment.PackageId,
                    StartDate = startDate,
                    EndDate = startDate.AddDays(30),
                    StatusId = 1, // Active
                    CreatedAt = DateTime.Now
                };

                _context.PackageSubscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                // Liên kết thanh toán với gói mới
                payment.SubscriptionId = subscription.SubscriptionId;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> RejectPaymentAsync(int paymentId, int adminUserId, string reason)
        {
            var payment = await _context.PackagePayments
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null || payment.StatusId != 1) // Không tồn tại hoặc không phải đang chờ
                return false;

            // Cập nhật trạng thái thanh toán
            payment.StatusId = 3; // Rejected

            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<PackagePaymentViewModel> GetPaymentsQuery(int statusId)
        {
            return _context.PackagePayments
                .Where(p => p.StatusId == statusId)
                .OrderByDescending(p => p.CreatedAt)
                .Join(_context.Users, p => p.UserId, u => u.UserId, (p, u) => new { Payment = p, User = u })
                .Join(_context.ServicePackages, pu => pu.Payment.PackageId, pk => pk.PackageId, (pu, pk) => new { pu.Payment, pu.User, Package = pk })
                .Join(_context.PaymentStatuses, pup => pup.Payment.StatusId, s => s.StatusId, (pup, s) => new { pup.Payment, pup.User, pup.Package, Status = s })
                .Select(x => MapToViewModel(x.Payment, x.User, x.Package, x.Status));
        }

        private static PackagePaymentViewModel MapToViewModel(PackagePayment payment, User user, ServicePackage package, PaymentStatus status)
        {
            return new PackagePaymentViewModel
            {
                PaymentId = payment.PaymentId,
                TransactionCode = payment.TransactionCode,
                UserId = user.UserId,
                UserFullName = user.FullName,
                UserEmail = user.Email,
                PackageId = package.PackageId,
                PackageName = package.PackageName,
                Amount = payment.Amount,
                StatusId = status.StatusId,
                StatusName = status.StatusName,
                CreatedAt = payment.CreatedAt,
                SubscriptionId = payment.SubscriptionId
            };
        }
    }
}

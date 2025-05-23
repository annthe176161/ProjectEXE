﻿using ProjectEXE.DTO;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Humanizer;
using ProjectEXE.ViewModel;

namespace ProjectEXE.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly RevaContext _context;
        public AdminService(RevaContext context)
        {
            _context = context;
        }

        public async Task<bool> addPackage(ServicePackage package)
        {
            try
            {
                _context.ServicePackages.Add(package);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { }
            return false;
        }

        public async Task<bool> deleteServiceById(int id)
        {
            var item = _context.ServicePackages.Find(id);
            if (item != null)
            {
                _context.ServicePackages.Remove(item);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<bool> editPackage(ServicePackage p)
        {
            var package = await _context.ServicePackages.FindAsync(p.PackageId);
            if (package == null) return false;

            package.PackageName = p.PackageName;
            package.ProductLimit = p.ProductLimit;
            package.Price = p.Price;
            package.DiscountedPrice = p.DiscountedPrice;

            _context.ServicePackages.Update(package);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ServicePackage>> getAllService()
        {
            return _context.ServicePackages.Include(s => s.PackageSubscriptions).ThenInclude(ps => ps.Status).ToList();
        }
        public async Task<List<RBMDto>> getRevenueByMonth()
        {
            var twelveMonthsAgo = DateTime.Now.AddMonths(-11);
            var firstDay = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1);

            var revenueData = await _context.PackagePayments
                .Where(p => p.CreatedAt >= firstDay && p.Status.StatusName == "Confirmed")
                .ToListAsync(); // Get data from database first

            var result = revenueData
                .GroupBy(p => p.CreatedAt.ToString("yyyy-MM"))
                .Select(g => new RBMDto
                {
                    RevenueMonth = g.Key,
                    TotalRevenue = g.Sum(p => p.Amount), // Remove division by 1000
                    FormattedRevenue = string.Format("{0:n0}", g.Sum(p => p.Amount)) // Format with thousand separators
                })
                .OrderBy(x => x.RevenueMonth)
                .ToList();

            return result;
        }

        public async Task<List<RBPDto>> getRevenueByPackage()
        {
            var twelveMonthsAgo = DateTime.Now.AddMonths(-11);
            var firstDay = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1);

            var revenueByPackage = await _context.PackagePayments
                .Where(p => p.CreatedAt >= firstDay && p.Status.StatusName == "Confirmed")
                .GroupBy(p => p.Package.PackageName)
                .Select(g => new RBPDto
                {
                    PackageName = g.Key,
                    TotalRevenue = g.Sum(p => p.Amount),  // Remove division by 1000
                    FormattedRevenue = string.Format("{0:n0}", g.Sum(p => p.Amount))  // Format with thousand separators
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToListAsync();

            return revenueByPackage;
        }


        public async Task<IEnumerable<RecentPackagePaymentDto>> GetRecentPackagePaymentsAsync(int count)
        {
            if (_context.PackagePayments == null)
            {
                return Enumerable.Empty<RecentPackagePaymentDto>();
            }

            var recentPayments = await _context.PackagePayments
                .AsNoTracking()
                .Include(pp => pp.User)
                .Include(pp => pp.Package)
                .Include(pp => pp.Status)
                .OrderByDescending(pp => pp.CreatedAt)
                .Take(count)
                .Select(pp => new RecentPackagePaymentDto // Đảm bảo đây là ProjectEXE.ViewModels.RecentPackagePaymentDto
                {
                    UserId = pp.UserId,
                    UserFullName = pp.User != null ? pp.User.FullName : "Không xác định",
                    PackageName = pp.Package != null ? pp.Package.PackageName : "Không xác định",
                    PricePaid = pp.Amount,
                    PaymentStatus = pp.Status != null ? pp.Status.StatusName : "Không xác định",
                    PaymentDate = pp.CreatedAt
                })
                .ToListAsync();

            return recentPayments;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kahoot.Common.BusinessResult;
using Kahoot.Common;
using Kahoot.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Kahoot.Service.Model.Response;
using Kahoot.Service.Interface;

namespace Kahoot.Service.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IBusinessResult> Revenue()
        {
            var userPackages = await _unitOfWork.UserPackageRepository
                .GetByWhere(x => x.StartDate != null && x.Package != null)
                .Include(x => x.Package)
                .ToListAsync();

            var dailyRevenue = userPackages
                .GroupBy(x => x.StartDate!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    PackageSold = g.Count(),
                    TotalRevenue = g.Sum(x => x.Package!.Price)
                })
                .OrderBy(x => x.Date)
                .ToList();


            var weeklyRevenue = userPackages
                .GroupBy(x => new
                {
                    Year = x.StartDate!.Value.Year,
                    Month = x.StartDate!.Value.Month,
                    Week = ((x.StartDate!.Value.Day - 1) / 7) + 1 
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Week = g.Key.Week,
                    PackageSold = g.Count(),
                    TotalRevenue = g.Sum(x => x.Package!.Price)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ThenBy(x => x.Week)
                .ToList();

            var monthlyRevenue = userPackages
                .GroupBy(x => new { Year = x.StartDate!.Value.Year, Month = x.StartDate!.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    PackageSold = g.Count(),
                    TotalRevenue = g.Sum(x => x.Package!.Price)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            var annualRevenue = userPackages
                .GroupBy(x => x.StartDate!.Value.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    PackageSold = g.Count(),
                    TotalRevenue = g.Sum(x => x.Package!.Price)
                })
                .OrderBy(x => x.Year)
                .ToList();

            var totalRevenue = userPackages
                .Where(x => x.StartDate != null)
                .ToList();

            var totalRevenueAmount = totalRevenue.Sum(x => x.Package!.Price);

            var totalPackageSold = totalRevenue.Count;

            var response = new RevenueResponse
            {
                Revenue = new
                {
                    Daily = dailyRevenue,
                    Weekly = weeklyRevenue,
                    Monthly = monthlyRevenue,
                    Annual = annualRevenue,
                    Total = new
                    {
                        PackageSold = totalPackageSold,
                        TotalRevenue = totalRevenueAmount
                    }
                }
            };

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);

        }

        public async Task<IBusinessResult> Transaction(int pageIndex, int pageSize, string? search)
        {
            search = search?.ToLower() ?? string.Empty;
            var transaction = await _unitOfWork.UserPackageRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                x => (string.IsNullOrEmpty(search) || x.User.Email.ToLower().Contains(search)
                                                   || x.Package.PackageName.ToLower().Contains(search))
                                                   || x.Package.Price.Equals(search),
                q => q.OrderByDescending(x => x.StartDate),
                i => i.Include(x => x.User).Include(x => x.Package)
                );

            var response = transaction.Select(x => new TransactionResponse
            {
                UserId = x.UserId,
                Email = x.User.Email,
                PackageId = x.PackageId,
                PackageName = x.Package.PackageName,
                Description = x.Package.Description,
                Price = x.Package.Price,
                PaidAt = x.StartDate,
                ExpiryDate = x.ExpiryDate
            });

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }
    }
}

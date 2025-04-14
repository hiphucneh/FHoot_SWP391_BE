using CloudinaryDotNet.Actions;
using Kahoot.Service.Helpers;
using Kahoot.Service.Interface;
using Kahoot.Service.ModelDTOs.Request;
using Kahoot.Service.ModelDTOs.Response;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;
using Kahoot.Common;
using Kahoot.Common.BusinessResult;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Services
{
    public class PackageService : IPackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TokenHandlerHelper _tokenHandlerHelper;
        public PackageService(IUnitOfWork unitOfWork, TokenHandlerHelper tokenHandlerHelper)
        {
            _unitOfWork = unitOfWork;
            _tokenHandlerHelper = tokenHandlerHelper;
        }

        public async Task<IBusinessResult> GetPackage()
        {
            var package = await _unitOfWork.PackageRepository.GetAll().OrderByDescending(x => x.CreatedAt).ToListAsync();
            if (package == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }
            var result = package.Adapt<List<PackageResponse>>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, result);
        }

        public async Task<IBusinessResult> GetPackageById(int packageId)
        {
            var package = await _unitOfWork.PackageRepository.GetByIdAsync(packageId);
            if (package == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }
            var result = package.Adapt<PackageResponse>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, result);
        }
        public async Task<IBusinessResult> CreatePackage(PackageRequest request)
        {
            var existingPackage = await _unitOfWork.PackageRepository
                .GetByWhere(p => p.PackageName == request.PackageName)
                .FirstOrDefaultAsync();
            if (existingPackage != null)
            {
                return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "Tên gói đã tồn tại");
            }

            var package = request.Adapt<Package>();
            package.CreatedAt = DateTime.UtcNow;
            package.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.PackageRepository.AddAsync(package);
            await _unitOfWork.SaveChangesAsync();

            var response = package.Adapt<PackageResponse>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG, response);
        }

        public async Task<IBusinessResult> UpdatePackage(int packageId, PackageRequest request)
        {

            var package = await _unitOfWork.PackageRepository.GetByIdAsync(packageId);
            if (package == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            var existingPackage = await _unitOfWork.PackageRepository
                .GetByWhere(p => p.PackageName == request.PackageName && p.PackageId != packageId)
                .FirstOrDefaultAsync();
            if (existingPackage != null)
            {
                return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "Tên gói đã tồn tại");
            }

            package.PackageName = request.PackageName;
            package.Price = request.Price;
            package.Duration = request.Duration;
            package.Description = request.Description;
            package.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.PackageRepository.UpdateAsync(package);
            await _unitOfWork.SaveChangesAsync();

            var response = package.Adapt<PackageResponse>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
        }

        public async Task<IBusinessResult> DeletePackage(int packageId)
        {
            var package = await _unitOfWork.PackageRepository.GetByIdAsync(packageId);
            if (package == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Gói không tồn tại");
            }

            await _unitOfWork.PackageRepository.DeleteAsync(package);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
        }

        public async Task<IBusinessResult> GetUserPackage(int pageIndex, int pageSize, string? status, string? search)
        {
            search = search?.ToLower() ?? string.Empty;

            var userPackages = await _unitOfWork.UserPackageRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                x => (string.IsNullOrEmpty(status) || x.Status.ToLower() == status.ToLower()) &&
                      (string.IsNullOrEmpty(search) || x.User.FullName.ToLower().Contains(search)
                                                   || x.Package.PackageName.ToLower().Contains(search)),
                q => q.OrderByDescending(x => x.StartDate),
                i => i.Include(x => x.User).Include(x => x.Package)
                );
            if (userPackages == null || !userPackages.Any())
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }
            foreach (var package in userPackages)
            {
                if (package.Status == "Active" && package.ExpiryDate <= DateTime.UtcNow)
                {
                    package.Status = "Expired";
                    await _unitOfWork.UserPackageRepository.UpdateAsync(package);
                }
            }
            await _unitOfWork.SaveChangesAsync();
            var response = userPackages.Select(up => new UserPackageResponse
            {
                UserPackageId = up.UserPackageId,
                UserId = up.UserId,
                FullName = up.User?.FullName,
                PackageId = up.PackageId,
                PackageName = up.Package?.PackageName,
                StartDate = up.StartDate,
                ExpiryDate = up.ExpiryDate,
                Status = up.Status
            }).ToList();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }

        public async Task<IBusinessResult> PayforPackage(string cancelUrl, string returnUrl, int packageId)
        {
            var userId = await _tokenHandlerHelper.GetUserId();
            var userPackagecheck = await _unitOfWork.UserPackageRepository
                .GetByWhere(x => x.UserId == userId && x.PackageId == packageId)
                .FirstOrDefaultAsync();

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

            // Nếu gói đã tồn tại nhưng trạng thái là "InActive", cập nhật và thực hiện lại
            if (userPackagecheck != null)
            {
                if (userPackagecheck.Status == "InActive")
                {

                    var package = await _unitOfWork.PackageRepository.GetByIdAsync(packageId);
                    if (package == null)
                    {
                        return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Gói không tồn tại");
                    }

                    // Tạo yêu cầu thanh toán mới
                    List<ItemData> itemData = new List<ItemData>
                                            {
                                                new ItemData(package.PackageName, 1, Convert.ToInt32(package.Price.Value))
                                            };
                    var result = await CreatePaymentRequestAsync(package, user, cancelUrl, returnUrl, itemData);

                    return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, result.Data);
                }

                return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "Gói đã tồn tại và đang hoạt động");
            }
            // Nếu gói chưa tồn tại, tạo mới
            var newUserPackage = new UserPackage
            {
                UserId = userId,
                PackageId = packageId,
                StartDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow,
                Status = "InActive"
            };
            await _unitOfWork.UserPackageRepository.AddAsync(newUserPackage);
            await _unitOfWork.SaveChangesAsync();

            var packageNew = await _unitOfWork.PackageRepository.GetByIdAsync(packageId);

            if (packageNew == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Gói không tồn tại");
            }

            List<ItemData> newItemData = new List<ItemData>
            {
                new ItemData(packageNew.PackageName, 1, Convert.ToInt32(packageNew.Price.Value))
            };
            var newResult = await CreatePaymentRequestAsync(packageNew, user, cancelUrl, returnUrl, newItemData);

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, newResult.Data);
        }


        public async Task<IBusinessResult> PAYOSCallback(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Trạng thái thanh toán không hợp lệ.");
            }

            var userid = await _tokenHandlerHelper.GetUserId();
            var userPackage = await _unitOfWork.UserPackageRepository
                .GetByWhere(x => x.UserId == userid && x.Status == "InActive")
                .Include(x => x.Package)  // Đảm bảo load Package
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefaultAsync();

            if (userPackage == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Gói không tồn tại");
            }

            if (status.ToLower() == "success" || status.ToLower() == "paid")
            {
                if (userPackage.StartDate.HasValue && userPackage.Package?.Duration.HasValue == true)
                {
                    userPackage.Status = "Active";
                    userPackage.ExpiryDate = userPackage.StartDate.Value.AddMonths(userPackage.Package.Duration.Value);
                    await _unitOfWork.UserPackageRepository.UpdateAsync(userPackage);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Dữ liệu không hợp lệ, không thể cập nhật ngày hết hạn.");
                }
            }

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG);
        }

        private async Task<IBusinessResult> CreatePaymentRequestAsync(Package package, User user, string cancelUrl, string returnUrl, List<ItemData> itemdata)
        {
            PayOS _payOS = new PayOS(Environment.GetEnvironmentVariable("PAYOS_CLIENTID"), Environment.GetEnvironmentVariable("PAYOS_APIKEY"), Environment.GetEnvironmentVariable("PAYOS_CHECKSUMKEY"));

            long orderCode = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            PaymentData paymentData = new PaymentData(orderCode, Convert.ToInt32(package.Price.Value), "Thanh toan don hang", itemdata, cancelUrl, returnUrl, null, user.FullName, user.Email, user.Phone, "Không có", DateTimeOffset.Now.AddMinutes(5).ToUnixTimeSeconds());

            CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG, createPayment.checkoutUrl);
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Kahoot.Common.BusinessResult;
using Kahoot.Common;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using Kahoot.Service.Helpers;
using Kahoot.Service.ModelDTOs.Response;
using Microsoft.AspNetCore.Http;
using Net.payOS.Types;
using Net.payOS;
using Kahoot.Service.Model.Response;
using Kahoot.Service.Interface;
using Kahoot.Service.Model.Request;
using Microsoft.EntityFrameworkCore;
using Mapster;
using NutriDiet.Service.Enums;
using System.Net.Mail;
using System.Net;

namespace Kahoot.Service.Services
{
    public class PackageService : IPackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TokenHandlerHelper _tokenHandlerHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _userIdClaim;
        public PackageService(IUnitOfWork unitOfWork, TokenHandlerHelper tokenHandlerHelper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _tokenHandlerHelper = tokenHandlerHelper;
            _httpContextAccessor = httpContextAccessor;
            _userIdClaim = GetUserIdClaim();
        }

        private string GetUserIdClaim()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
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

        public async Task<IBusinessResult> GetMyUserPackage()
        {
            int userid = int.Parse(_userIdClaim);
            var userPackage = await _unitOfWork.UserPackageRepository.GetByWhere(x => x.UserId == userid && x.Status == "Active" && x.ExpiryDate > DateTime.Now)
                .FirstOrDefaultAsync();

            if (userPackage == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            var response = userPackage.Adapt<UserPackageResponse>();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }

        public async Task<IBusinessResult> PayforPackage(string cancelUrl, string returnUrl, int packageId)
        {
            var userId = await _tokenHandlerHelper.GetUserId();
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            // Kiểm tra gói hiện tại của người dùng
            var userPackageCheck = await _unitOfWork.UserPackageRepository
                .GetByWhere(x => x.UserId == userId && x.Status == "Active" && x.ExpiryDate > now)
                .Include(x => x.Package)
                .FirstOrDefaultAsync();

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

            // Nếu có gói đang hoạt động
            if (userPackageCheck != null || user.RoleId.Equals((int)RoleEnum.Teacher))
            {
                return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "Bạn đang có gói Basic Premium đang hoạt động. Bạn đang là host");
            }

            // Lấy thông tin gói mới
            var package = await _unitOfWork.PackageRepository.GetByIdAsync(packageId);
            if (package == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Gói không tồn tại");
            }

            // Tạo bản ghi UserPackage mới
            var newUserPackage = new UserPackage
            {
                UserId = userId,
                PackageId = packageId,
                StartDate = now,
                ExpiryDate = now,
                Status = "InActive"
            };
            await _unitOfWork.UserPackageRepository.AddAsync(newUserPackage);
            await _unitOfWork.SaveChangesAsync();

            // Tạo yêu cầu thanh toán PayOS
            List<ItemData> itemData = new List<ItemData>
            {
                new ItemData(package.PackageName, 1, Convert.ToInt32(package.Price.Value))
            };
            var result = await CreatePaymentRequestAsync(package, user, cancelUrl, returnUrl, itemData);

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, result.Data);
        }

        public async Task<IBusinessResult> PAYOSCallback(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Trạng thái thanh toán không hợp lệ.");
            }

            var userId = await _tokenHandlerHelper.GetUserId();
            var userPackage = await _unitOfWork.UserPackageRepository
                .GetByWhere(x => x.UserId == userId && x.Status == "InActive")
                .Include(x => x.Package)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefaultAsync();

            if (userPackage == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Gói không tồn tại.");
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "User không tồn tại.");
            }

            if (status.ToLower() == "success" || status.ToLower() == "paid")
            {
                if (userPackage.StartDate.HasValue && userPackage.Package?.Duration.HasValue == true)
                {
                    userPackage.Status = "Active";
                    user.RoleId = (int)RoleEnum.Teacher;
                    userPackage.ExpiryDate = userPackage.StartDate.Value.AddDays(userPackage.Package.Duration.Value);
                    await _unitOfWork.UserPackageRepository.UpdateAsync(userPackage);
                    await _unitOfWork.SaveChangesAsync();
                    await SendPaymentSuccessEmail(user.Email, userPackage);
                }
                else
                {
                    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Dữ liệu không hợp lệ, không thể cập nhật ngày hết hạn.");
                }
            }

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG);
        }

        private async Task SendPaymentSuccessEmail(string email, UserPackage userPackage)
        {
            try
            {
                var packageName = userPackage.Package?.PackageName ?? "Không xác định";
                var price = userPackage.Package?.Price ?? 0;
                var startDate = userPackage.StartDate?.ToString("dd/MM/yyyy") ?? "-";
                var expiryDate = userPackage.ExpiryDate.ToString("dd/MM/yyyy") ?? "-";

                var emailContent = $@"
<style>
    body {{
        font-family: Arial, sans-serif;
        color: #333;
    }}
    h2 {{
        color: #FFA500;
    }}
    table {{
        width: 100%;
        border-collapse: collapse;
        margin-top: 20px;
    }}
    th, td {{
        border: 1px solid #ddd;
        padding: 10px;
        text-align: left;
    }}
    th {{
        background-color: #FFA500;
        color: white;
    }}
    tr:nth-child(even) {{
        background-color: #f9f9f9;
    }}
    a {{
        color: #007BFF;
        text-decoration: none;
    }}
</style>

<p>Xin chào <a href='mailto:{email}'>{email}</a>,</p>
<p>Bạn đã thanh toán thành công gói dịch vụ trên hệ thống <strong>Kahoot</strong>.</p>

<table>
    <thead>
        <tr>
            <th>Tên gói</th>
            <th>Giá</th>
            <th>Ngày bắt đầu</th>
            <th>Ngày hết hạn</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>{packageName}</td>
            <td>{price:N0} VNĐ</td>
            <td>{startDate}</td>
            <td>{expiryDate}</td>
        </tr>
    </tbody>
</table>

<p>Xin cảm ơn bạn đã tin tưởng sử dụng dịch vụ của chúng tôi.</p>
<p>Trân trọng,</p>
<p><strong>Đội ngũ hỗ trợ Kahoot</strong></p>
<p>Website: <a href='https://kahoot-fe-nu.vercel.app'>https://kahoot-fe-nu.vercel.app</a></p>
";


                var emailSender = Environment.GetEnvironmentVariable("EMAIL_SENDER");
                var emailSenderPassword = Environment.GetEnvironmentVariable("EMAIL_SENDER_PASSWORD");

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("support@kahoot.com", "Kahoot Support Team"),
                    Subject = "Xác nhận thanh toán thành công",
                    Body = emailContent,
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(emailSender, emailSenderPassword);

                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Gửi email thất bại: " + ex.Message);
            }
        }

        private async Task<IBusinessResult> CreatePaymentRequestAsync(Package package, User user, string cancelUrl, string returnUrl, List<ItemData> itemdata)
        {
            PayOS _payOS = new PayOS(Environment.GetEnvironmentVariable("PAYOS_CLIENTID"), Environment.GetEnvironmentVariable("PAYOS_APIKEY"), Environment.GetEnvironmentVariable("PAYOS_CHECKSUMKEY"));

            long orderCode = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int totalAmount = itemdata[0].price;
            PaymentData paymentData = new PaymentData(orderCode, totalAmount, "Thanh toan don hang", itemdata, cancelUrl, returnUrl, null, user.FullName, user.Email, "", "Không có", DateTimeOffset.Now.AddMinutes(5).ToUnixTimeSeconds());

            CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG, createPayment.checkoutUrl);
        }

    }
}

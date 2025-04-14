using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Utilities
{
    public class GoogleService
    {
        private readonly IMemoryCache _cache;

        public GoogleService(IMemoryCache cache)
        {
            _cache = cache;

        }
        public async Task SendEmail(string email, string subject, string body)
        {
            try
            {
                var emailSender = Environment.GetEnvironmentVariable("EMAIL_SENDER");
                var emailSenderPassword = Environment.GetEnvironmentVariable("EMAIL_SENDER_PASSWORD");

                if (string.IsNullOrEmpty(emailSender) || string.IsNullOrEmpty(emailSenderPassword))
                {
                    throw new Exception("Email sender credentials are not configured.");
                }

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("support@nutridiet.com", "NutriDiet Support Team"),
                    Subject = subject,
                    Body = body ?? "No content available",
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(emailSender, emailSenderPassword);
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error when sending email: " + ex.Message);
            }
        }


        public async Task SendEmailWithOTP(string email, string subject)
        {
            try
            {
                var otp = GenerateOtp();

                _cache.Set(email, otp, TimeSpan.FromMinutes(5));

                // Cải thiện nội dung email
                var emailContent = $@"
                            <p>Xin chào {email},</p>
                            <p>Bạn nhận được email này vì đã yêu cầu mã OTP để đăng nhập vào tài khoản NutriDiet của mình.</p>
                            <p>Mã OTP của bạn là: <strong>{otp}</strong></p>
                            <p>Mã này có hiệu lực trong 5 phút.</p>
                            <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email.</p>
                            <p>Trân trọng,</p>
                            <p>Đội ngũ hỗ trợ NutriDiet</p>
                            <p>Website: https://www.nutridiet.live/</p>
                            ";

                var emailSender = Environment.GetEnvironmentVariable("EMAIL_SENDER");
                var emailSenderPassword = Environment.GetEnvironmentVariable("EMAIL_SENDER_PASSWORD");

                // Sử dụng tên miền riêng (thay "no-reply@yourdomain.com" bằng tên miền của bạn)
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("support@nutridiet.com", "NutriDiet Support Team"),
                    Subject = subject,
                    Body = emailContent,
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                // Gửi email qua SMTP
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
                throw new Exception("Error when sending email: " + ex.Message);
            }
        }

        public async Task<bool> VerifyOtp(string email, string otp)
        {
            var cachedOtp = _cache.Get(email) as string;

            Console.WriteLine($"Cached OTP: {cachedOtp} for email: {email}");
            Console.WriteLine($"Provided OTP: {otp}");

            if (!string.IsNullOrEmpty(cachedOtp) && cachedOtp.Equals(otp))
            {
                _cache.Remove(email);
                return true;
            }

            return false;
        }


        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<string> UploadImageGGDrive(IFormFile file)
        {
            // Lấy dữ liệu JSON từ biến môi trường
            var credentialsJson = Environment.GetEnvironmentVariable("GOOGLE_CREDENTIALS_JSON");

            if (string.IsNullOrEmpty(credentialsJson))
            {
                throw new Exception("Google credentials JSON not found in environment variables.");
            }

            GoogleCredential credential;

            try
            {
                credential = GoogleCredential.FromJson(credentialsJson)
                    .CreateScoped(new[] { DriveService.ScopeConstants.DriveFile });

                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "NutriDiet Upload App"
                });

                var fileMetaData = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = file.FileName,
                    Parents = new List<string> { "1ut6fCcK_V8x9G81AXov98ob37_1dtv-U" }
                };

                FilesResource.CreateMediaUpload request;

                using (var streamFile = file.OpenReadStream())
                {
                    request = service.Files.Create(fileMetaData, streamFile, file.ContentType);
                    request.Fields = "id";
                    var progress = await request.UploadAsync();

                    if (progress.Status == UploadStatus.Failed)
                    {
                        throw new Exception($"File upload failed: {progress.Exception.Message}");
                    }

                    var uploadedFile = request.ResponseBody;
                    var fileUrl = $"https://drive.google.com/thumbnail?id={uploadedFile.Id}";
                    return fileUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during file upload: {ex.Message}");
                return null;
            }
        }

    }
}

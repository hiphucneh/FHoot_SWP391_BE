using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Kahoot.Common;
using Kahoot.Common.BusinessResult;
using Kahoot.Common.Enums;
using Kahoot.Repository.Interface;
using System.Security.Claims;

namespace Kahoot.Service.Utilities
{
    public class FirebaseService
    {
        private static FirebaseApp? _firebaseApp;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _userIdClaim;

        public FirebaseService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            if (_firebaseApp == null)
            {
                string? firebase = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
                _firebaseApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(firebase)
                });
            }
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _userIdClaim = GetUserIdClaim();
        }

        private string GetUserIdClaim()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        public async Task SendNotification(string fcmToken, string? title, string body)
        {
            var message = new Message()
            {
                Token = fcmToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                }
            };

            var userId = await GetUserIdByFcmToken(fcmToken);

            if (userId == 0)
            {
                Console.WriteLine("User not found for FCM token: " + fcmToken);
                return;
            }

            try
            {
                Console.WriteLine("Sending notification to: " + fcmToken);
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine("Notification sent successfully: " + response);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending notification: " + ex.Message);
            }
        }

        public async Task<int> GetUserIdByFcmToken(string fcmToken)
        {
            try
            {
                var user = await _unitOfWork.UserRepository
                    .GetByWhere(u => u.FcmToken == fcmToken).FirstOrDefaultAsync();

                return user.UserId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving UserId by FCM token: {ex.Message}");
                return 0;
            }
        }

        public async Task<IBusinessResult> EnableReminder()
        {
            int userId = int.Parse(_userIdClaim);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "User not found");
            }

            if (string.IsNullOrEmpty(user.FcmToken))
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "FCM token is required");
            }


            // EnableReminder flag
            user.EnableReminder = !(user.EnableReminder ?? false);
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            string message = user.EnableReminder.Value
                ? $"Reminder enabled"
                : $"Reminder disabled";

            return new BusinessResult(Const.HTTP_STATUS_OK, message);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Kahoot.Repository.Repositories
{
    public class UserPackageRepository : GenericRepository<UserPackage>, IUserPackageRepository
    {
        public UserPackageRepository(KahootContext context) : base(context) { }

        public async Task<bool> IsUserPremiumAsync(int userId)
        {
            var activePackage = await GetByWhere(up => up.UserId == userId && up.Status == "Active" && up.ExpiryDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();
            return activePackage != null;
        }

        public async Task<bool> IsUserAdvancedPremiumAsync(int userId)
        {
            var activePackage = await GetByWhere(up => up.UserId == userId && up.Status == "Active" && up.ExpiryDate > DateTime.UtcNow)
                .Include(up => up.Package)
                .FirstOrDefaultAsync();
            return activePackage != null && activePackage.Package.PackageType == "Advanced";
        }
    }
}

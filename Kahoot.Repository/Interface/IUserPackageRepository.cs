using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kahoot.Repository.Base;
using Kahoot.Repository.Models;

namespace Kahoot.Repository.Interface
{
    public interface IUserPackageRepository : IGenericRepository<UserPackage>
    {
        Task<bool> IsUserPremiumAsync(int userId);
        Task<bool> IsUserAdvancedPremiumAsync(int userId);
    }
}

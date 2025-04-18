using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.ModelDTOs.Response
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public int? Age { get; set; }

        public string? Gender { get; set; }

        public string? Location { get; set; }

        public string? Avatar { get; set; }

        public string? Status { get; set; }
        public List<UserPackagesResponse> UserPackages { get; set; }
    }
    public class UserPackagesResponse
    {
        public int UserPackageId { get; set; }

        public int PackageId { get; set; }
        public string PackageName { get; set; } = null!;

        public DateTime? StartDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string? Status { get; set; }
    }
}

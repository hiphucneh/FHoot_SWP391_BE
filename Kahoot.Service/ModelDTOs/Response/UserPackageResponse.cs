using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.ModelDTOs.Response
{
    public class UserPackageResponse
    {
        public int UserPackageId { get; set; }

        public int UserId { get; set; }
        public string FullName { get; set; } = null!;

        public int PackageId { get; set; }
        public string PackageName { get; set; } = null!;

        public DateTime? StartDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string? Status { get; set; }
    }
}

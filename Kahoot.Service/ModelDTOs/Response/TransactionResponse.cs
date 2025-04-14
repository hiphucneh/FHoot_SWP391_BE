using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.ModelDTOs.Response
{
    public class TransactionResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public int PackageId { get; set; }
        public string PackageName { get; set; } = null!;
        public string? Description { get; set; }
        public double? Price { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Response
{
    public class PackageResponse
    {
        public int PackageId { get; set; }
        public string PackageType { get; set; }

        public string PackageName { get; set; } = null!;

        public double? Price { get; set; }

        public int? Duration { get; set; }

        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

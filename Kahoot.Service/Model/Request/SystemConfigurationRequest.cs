using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Request
{
    public class SystemConfigurationRequest
    {
        public string Name { get; set; } = null!;

        public double? MinValue { get; set; }

        public double? MaxValue { get; set; }

        public string? Unit { get; set; }

        public bool? IsActive { get; set; }

        public string? Description { get; set; }
    }
}

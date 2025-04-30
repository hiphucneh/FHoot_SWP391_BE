using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Request
{
    public class PackageRequest
    {
        [Required(ErrorMessage = "Package name is required")]
        public string PackageName { get; set; } = null!;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue,
            ErrorMessage = "Price must be greater than 0")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Duration must be greater than 0")]
        public int Duration { get; set; }

        public string? Description { get; set; }
    }
}

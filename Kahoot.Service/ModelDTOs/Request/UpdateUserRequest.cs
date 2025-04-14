using Microsoft.AspNetCore.Http;
using Kahoot.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace Kahoot.Service.ModelDTOs.Request
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Age is required")]
        [Range(13, 100,
            ErrorMessage = "Age must be between 13 and 100")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }

        public IFormFile? Avatar { get; set; }
    }
}

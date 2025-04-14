using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.ModelDTOs.Request
{
    public class ResetPasswordRequest
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string OTP { get; set; }
        public required string NewPassword { get; set; }
    }
}

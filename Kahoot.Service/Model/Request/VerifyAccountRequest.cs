using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.ModelDTOs.Request
{
    public class VerifyAccountRequest
    {
        [EmailAddress]
        public string Email { get; set; }
        public string OTP { get; set; }
    }
}

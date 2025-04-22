using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Request
{
    public class JoinTeamRequest
    {
        public int teamId { get; set; }
        public string FullName { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }
}

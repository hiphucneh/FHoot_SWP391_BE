using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.ModelDTOs.Request
{
    public class ImageUpload
    {
        public IFormFile ImgUrl { get; set; }
    }
}

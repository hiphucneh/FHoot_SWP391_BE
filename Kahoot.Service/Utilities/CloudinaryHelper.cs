using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Utilities
{
    public class CloudinaryHelper
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryHelper()
        {
            _cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            _cloudinary.Api.Secure = true;
        }
        public async Task DeleteImage(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Result != "ok")
            {
                throw new Exception($"Failed to delete image from Cloudinary: {result.Error?.Message}");
            }
        }

        public async Task<string> UploadImageWithCloudDinary(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(Guid.NewGuid().ToString(), stream),
                    Folder = "images"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return uploadResult.SecureUrl.ToString();
                }
                else
                {
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
                }
            }
        }

        public async Task<string> UpdateImageWithCloudinary(string publicId, IFormFile newFile)
        {
            var deletionParams = new DeletionParams(publicId);
            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.Result != "ok")
            {
                throw new Exception($"Cloudinary deletion failed: {deletionResult.Error?.Message}");
            }

            using (var stream = newFile.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(Guid.NewGuid().ToString(), stream),
                    PublicId = publicId,
                    Folder = "images"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return uploadResult.SecureUrl.ToString();
                }
                else
                {
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error?.Message}");
                }
            }
        }
    }
}

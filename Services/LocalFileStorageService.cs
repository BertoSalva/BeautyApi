using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class LocalFileStorageService
    {
        private readonly string _uploadFolder;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env; // ✅ add this field

        public LocalFileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env; // ✅ assign it here
            _uploadFolder = Path.Combine(env.WebRootPath, "Uploads");
            _httpContextAccessor = httpContextAccessor;

            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            string fullPath = Path.Combine(_uploadFolder, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var request = _httpContextAccessor.HttpContext.Request;
            string baseUrl = $"{request.Scheme}://{request.Host}";

            return $"{baseUrl}/Uploads/{uniqueFileName}";
        }

        public string GetFilePath(string fileUrl)
        {
            return Path.Combine(_env.WebRootPath, fileUrl.TrimStart('/'));
        }
    }
}

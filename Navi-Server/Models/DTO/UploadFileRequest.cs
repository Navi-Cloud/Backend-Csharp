using Microsoft.AspNetCore.Http;

namespace Navi_Server.Models.DTO
{
    public class UploadFileRequest
    {
        public IFormFile File { get; set; }
        public string ParentFolder { get; set; }
    }
}
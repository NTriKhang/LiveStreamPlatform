using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace BackendNet.Services.IService
{
    public interface IAwsService
    {
        Task<string> UploadImage(IFormFile formFiles);
        Task<HttpStatusCode> UploadStreamVideo(string streamkey, string folderContainName);
        Task<HttpStatusCode> DeleteVideo(string videoId);
        string GenerateVideoPostPresignedUrl(string videoId, long videoSize);

    }
}

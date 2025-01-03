﻿using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.SecurityToken.Model;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Headers;
using System.IO;
using Amazon.S3.Model;
using SharpCompress.Common;

namespace BackendNet.Services
{
    public class AwsService : IService.IAwsService
    {
        private IConfiguration configuration;
        public AwsService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        private AmazonS3Client getS3Client()
        {
            BasicAWSCredentials basicAWSCredentials =
                new BasicAWSCredentials(
                    configuration.GetSection("AwsCredentail").GetValue<string>("AccessKey") ?? "",
                    configuration.GetSection("AwsCredentail").GetValue<string>("SecretKey") ?? ""
                );
            AmazonS3Client s3Client = new AmazonS3Client(basicAWSCredentials, Amazon.RegionEndpoint.APSoutheast2);
            return s3Client;
        }
        public string GenerateVideoPostPresignedUrl(string videoId, long videoSize)
        {
            try
            {
                var s3Client = getS3Client();
                if (s3Client == null)
                    return string.Empty;
                string? bucketName = configuration.GetSection("BucketName").GetValue<string>("EduVideo");
                double timeout = 2;

                if (bucketName == null)
                {
                    return string.Empty;
                }
                var request = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = videoId,
                    Expires = DateTime.UtcNow.AddMinutes(timeout),
                    Verb = HttpVerb.PUT
                };
                var urlString = s3Client.GetPreSignedURL(request);
                return urlString;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error:'{ex.Message}'");
                throw;
            }
        }
        public string GenerateVideoDeletePresignedUrl(string videoId)
        {
            try
            {
                var s3Client = getS3Client();
                if (s3Client == null)
                    return string.Empty;
                string? bucketName = configuration.GetSection("BucketName").GetValue<string>("EduVideo");
                double timeout = 2;

                if (bucketName == null)
                {
                    return string.Empty;
                }
                var request = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = videoId,
                    Expires = DateTime.UtcNow.AddMinutes(timeout),
                    Verb = HttpVerb.DELETE
                };
                var urlString = s3Client.GetPreSignedURL(request);
                return urlString;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error:'{ex.Message}'");
                throw;
            }
        }
        public async Task<string> UploadImage(IFormFile formFiles)
        {
            try
            {
                string name = Guid.NewGuid().ToString().Substring(0,6) + formFiles.FileName;
                byte[] thumbnailData = null;
                if (formFiles != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await formFiles.CopyToAsync(memoryStream);
                        thumbnailData = memoryStream.ToArray();
                    }
                }
                using (var client = new HttpClient())
                {
                    string apiUrl = configuration.GetValue<string>("ImageApiGateWay")!;
                    var content = new ByteArrayContent(thumbnailData);
                    content.Headers.ContentType = new MediaTypeHeaderValue(formFiles.ContentType);

                    var response = await client.PutAsync(apiUrl + name, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }
                }
                return name;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<HttpStatusCode> UploadStreamVideo(string streamkey, string folderContainName)
        {
            string filePathConfig = configuration.GetValue<string>("FilePath")!;
            var filePath = filePathConfig + streamkey;

            string accesckey = configuration.GetSection("AwsCredentail").GetValue<string>("AccessKey");
            BasicAWSCredentials basicAWSCredentials =
                new BasicAWSCredentials(
                    configuration.GetSection("AwsCredentail").GetValue<string>("AccessKey"),
                    configuration.GetSection("AwsCredentail").GetValue<string>("SecretKey")
                );

            //string destinationFolderName = "D:\\Docker\\LiveStreamPlatform\\BackendNet\\wwwroot\\" + streamkey + "\\";
            //Directory.CreateDirectory(destinationFolderName);
            AmazonS3Client s3Client = new AmazonS3Client(basicAWSCredentials, Amazon.RegionEndpoint.APSoutheast2);
            try
            {
                string bucketName = "edunimovideov2";
                await CreateS3Folder(s3Client, bucketName, folderContainName);

                TransferUtility transferUtility = new TransferUtility(s3Client);
                var request = new TransferUtilityUploadDirectoryRequest
                {
                    BucketName = bucketName,
                    Directory = filePath,
                    KeyPrefix = folderContainName,
                };
                await transferUtility.UploadDirectoryAsync(request);
                return HttpStatusCode.NoContent;
            }
            catch (Exception e)
            {
                Console.WriteLine(
                "Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
                return HttpStatusCode.InternalServerError;
            }
        }
        private async Task CreateS3Folder(AmazonS3Client s3Client, string bucketName, string folderName)
        {

            var request = new PutObjectRequest();
            request.BucketName = bucketName;
            request.StorageClass = S3StorageClass.Standard;
            request.ServerSideEncryptionCustomerMethod = ServerSideEncryptionCustomerMethod.None;
            request.Key = folderName + "/";
            request.ContentBody = string.Empty;
            var res = await s3Client.PutObjectAsync(request);
        }

        public async Task<HttpStatusCode> DeleteVideo(string videoId)
        {
            try
            {
                string bucketName = configuration.GetSection("BucketName").GetValue<string>("EduVideo") ?? "";
                var res = await DeleteObjectAsync(getS3Client(), bucketName , videoId);
                //if(res)
                return res;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<HttpStatusCode> DeleteObjectAsync(IAmazonS3 client, string bucketName, string keyName)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                };

                var res = await client.DeleteObjectAsync(deleteObjectRequest);
                return res.HttpStatusCode;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' when deleting an object.");
                throw;
            }
        }
    }
}

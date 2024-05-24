using BackendNet.Dtos;
using BackendNet.Hubs;
using BackendNet.Models;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Net;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamsController : ControllerBase
    {
        private readonly IStreamService _streamService;
        public StreamsController(IStreamService streamService)
        {
            _streamService = streamService;
        }
        [HttpPost("on_publish_done")]
        public async Task<int> OnPublishDone()
        {
            string requestbody = await Request.Body.ReadAsStringAsync();
            await _streamService.onPublishDone(requestbody);
            return 200;
        }
        [HttpPost("on_publish")]
        public async Task<int> OnPublish()
        {
            string requestbody = await Request.Body.ReadAsStringAsync();

            bool isValid = await _streamService.onPublish(requestbody);
            if (isValid)
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                return 200;
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return 400;
            }
        }
    }
    public static class RequestExtensions
    {
        public static async Task<string> ReadAsStringAsync(this Stream requestBody, bool leaveOpen = false)
        {
            using StreamReader reader = new(requestBody, leaveOpen: leaveOpen);
            var bodyAsString = await reader.ReadToEndAsync();
            return bodyAsString;
        }
    }
}

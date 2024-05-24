namespace BackendNet.Services.IService
{
    public interface IStreamService
    {
        public Task onPublishDone(string requestBody);
        public Task<bool> onPublish(string requestBody);
        Task removeStreamVideo(string streamKey);

    }
}

namespace BackendNet.Services.IService
{
    public interface IStreamService
    {
        Task onPublishDone(string requestBody);
        Task<bool> onPublish(string requestBody);
        Task removeStreamVideo(string streamKey);

    }
}

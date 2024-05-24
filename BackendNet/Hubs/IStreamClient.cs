namespace BackendNet.Hubs
{
    public interface IStreamClient
    {
        Task OnStopStreaming(string message);
        Task OnStartStreaming(string message);
    }
}

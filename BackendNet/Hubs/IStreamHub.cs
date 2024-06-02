namespace BackendNet.Hubs
{
    public interface IStreamHub
    {
        Task OnStopStreaming(string message);
        Task OnStartStreaming(string message);
    }
}

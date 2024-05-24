namespace BackendNet.DAL
{
    public class LiveStreamDatabaseSetting : ILiveStreamDatabaseSetting
    {
        public string ConnectionString { get; set; } = null;
        public string DatabaseName { get; set; } = null;

    }
}

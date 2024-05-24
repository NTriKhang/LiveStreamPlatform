namespace BackendNet.DAL
{
    public interface ILiveStreamDatabaseSetting
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}

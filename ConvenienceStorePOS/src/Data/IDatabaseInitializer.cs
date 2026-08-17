namespace ConvenienceStorePOS.Data
{
    public interface IDatabaseInitializer
    {
        Task InitializeAsync();
        string ConnectionString { get; }
    }
}

namespace BuildingBlocks.VectorDB;

public class VectorStoreOptions
{
    public string Host { get; set; } = "localhost";
    public string? ApiKey { get; set; }
    public bool UseHttps { get; set; }
    public int Port { get; set; } = 6334;
}

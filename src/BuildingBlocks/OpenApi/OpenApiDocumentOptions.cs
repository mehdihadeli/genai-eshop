namespace BuildingBlocks.OpenApi;

public class OpenApiDocumentOptions
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public Uri? AuthorUrl { get; set; }
    public string AuthorEmail { get; set; } = string.Empty;
    public string LicenseName { get; set; } = "MIT";
    public Uri LicenseUrl { get; set; } = new("https://opensource.org/licenses/MIT");
    public static bool IsOpenApiBuild => Environment.GetEnvironmentVariable("OpenApiBuild") == "true";
}

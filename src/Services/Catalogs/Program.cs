using Aspire.ServiceDefaults;
using GenAIEshop.Catalogs.Shared;
using GenAIEshop.Catalogs.Shared.Extensions.HostApplicationBuilderExtensions;
using GenAIEshop.Catalogs.Shared.Extensions.WebApplicationExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.UseInfrastructure();

app.MapDefaultEndpoints();

app.MapApplicationEndpoints();

app.Run();

public partial class Program { }

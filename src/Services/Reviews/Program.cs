using Aspire.ServiceDefaults;
using GenAIEshop.Reviews.Shared;
using GenAIEshop.Reviews.Shared.Extensions.HostApplicationBuilderExtensions;
using GenAIEshop.Reviews.Shared.Extensions.WebApplicationExtensions;

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

using Aspire.ServiceDefaults;
using GenAIEshop.Recommendation.Shared;
using GenAIEshop.Recommendation.Shared.Extensions.HostApplicationBuilderExtensions;
using GenAIEshop.Recommendation.Shared.Extensions.WebApplicationExtensions;

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

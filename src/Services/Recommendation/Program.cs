using Aspire.ServiceDefaults;
using BuildingBlocks.Env;
using GenAIEshop.Recommendation.Shared;
using GenAIEshop.Recommendation.Shared.Extensions.HostApplicationBuilderExtensions;
using GenAIEshop.Recommendation.Shared.Extensions.WebApplicationExtensions;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.UseInfrastructure();

app.MapDefaultEndpoints();

app.MapApplicationEndpoints();

app.Run();

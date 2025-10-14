using Aspire.ServiceDefaults;
using BuildingBlocks.Env;
using GenAIEshop.Reviews.Shared;
using GenAIEshop.Reviews.Shared.Extensions.HostApplicationBuilderExtensions;
using GenAIEshop.Reviews.Shared.Extensions.WebApplicationExtensions;

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

using Aspire.ServiceDefaults;
using BuildingBlocks.Env;
using GenAIEshop.Catalogs.Shared;
using GenAIEshop.Catalogs.Shared.Extensions.HostApplicationBuilderExtensions;
using GenAIEshop.Catalogs.Shared.Extensions.WebApplicationExtensions;

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

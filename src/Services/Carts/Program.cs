using Aspire.ServiceDefaults;
using BuildingBlocks.Env;
using GenAIEshop.Carts.Shared;
using GenAIEshop.Carts.Shared.Extensions.HostApplicationBuilderExtensions;
using GenAIEshop.Carts.Shared.Extensions.WebApplicationExtensions;

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

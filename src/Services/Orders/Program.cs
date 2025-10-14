using Aspire.ServiceDefaults;
using BuildingBlocks.Env;
using GenAIEshop.Orders.Shared;
using GenAIEshop.Orders.Shared.Extensions.HostApplicationBuilderExtensions;
using GenAIEshop.Orders.Shared.Extensions.WebApplicationExtensions;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddInfrastructure();

builder.AddApplicationServices();

var app = builder.Build();

app.UseInfrastructure();

app.MapDefaultEndpoints();

app.MapApplicationEndpoints();

app.Run();

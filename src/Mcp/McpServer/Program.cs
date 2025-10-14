using Aspire.ServiceDefaults;
using GenAIEshop.Carts.Shared;
using GenAIEshop.Carts.Shared.Extensions.HostApplicationBuilderExtensions;
using GenAIEshop.Carts.Shared.Extensions.WebApplicationExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.UseInfrastructure();

app.MapDefaultEndpoints();

app.MapApplicationEndpoints();

app.Run();

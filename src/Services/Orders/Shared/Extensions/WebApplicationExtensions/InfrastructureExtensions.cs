using BuildingBlocks.OpenApi;

namespace GenAIEshop.Orders.Shared.Extensions.WebApplicationExtensions;

public static class InfrastructureExtensions
{
    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions { AllowStatusCode404Response = true });
        // Handles non-exceptional status codes (e.g., 404 from Results.NotFound(), 401 from unauthorized access) and returns standardized ProblemDetails responses
        app.UseStatusCodePages();

        app.UseAspnetOpenApi();

        if (app.Environment.IsProduction())
        {
            app.UseHttpsRedirection();
        }

        return app;
    }
}

using System.Net.Mime;
using System.Text;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.OpenApi;

public static class OpenApiOptionsExtensions
{
    public static void ApplyApiVersionInfo(this OpenApiOptions options, OpenApiDocumentOptions? openApiDocumentOptions)
    {
        options.AddDocumentTransformer(
            (document, context, _) =>
            {
                var apiDescription = context
                    .ApplicationServices.GetService<IApiVersionDescriptionProvider>()
                    ?.ApiVersionDescriptions.SingleOrDefault(versionDescription =>
                        versionDescription.GroupName == context.DocumentName
                    );

                if (apiDescription is null)
                {
                    return Task.CompletedTask;
                }

                document.Info.License = new OpenApiLicense
                {
                    Name = openApiDocumentOptions?.LicenseName,
                    Url = openApiDocumentOptions?.LicenseUrl,
                };

                document.Info.Contact = new OpenApiContact
                {
                    Name = openApiDocumentOptions?.AuthorName,
                    Url = openApiDocumentOptions?.AuthorUrl,
                    Email = openApiDocumentOptions?.AuthorEmail,
                };

                document.Info.Version = apiDescription.ApiVersion.ToString();

                document.Info.Title = openApiDocumentOptions?.Title;

                document.Info.Description = BuildDescription(apiDescription, openApiDocumentOptions?.Description);

                return Task.CompletedTask;
            }
        );
    }

    private static string BuildDescription(ApiVersionDescription api, string? description)
    {
        var text = new StringBuilder(description ?? string.Empty);

        if (api.IsDeprecated)
        {
            text.AppendLine();
            text.AppendLine("**This API version has been deprecated.**");
        }

        if (api.SunsetPolicy is not { } policy)
        {
            return text.ToString();
        }

        text.AppendLine();

        if (policy.Date is { } when)
        {
            text.AppendLine($"**Sunset date:** {when:yyyy-MM-dd}");
        }

        if (!policy.HasLinks)
        {
            return text.ToString();
        }

        text.AppendLine();

        var rendered = false;

        foreach (var link in policy.Links.Where(l => l.Type == MediaTypeNames.Text.Html))
        {
            if (!rendered)
            {
                text.Append("<h4>Links</h4><ul>");
                rendered = true;
            }

            text.Append("<li><a href=\"");
            text.Append(link.LinkTarget.OriginalString);
            text.Append("\">");
            text.Append(
                StringSegment.IsNullOrEmpty(link.Title) ? link.LinkTarget.OriginalString : link.Title.ToString()
            );
            text.Append("</a></li>");
        }

        if (rendered)
        {
            text.Append("</ul>");
        }

        return text.ToString();
    }

    public static void ApplySchemaNullableFalse(this OpenApiOptions documentOptions)
    {
        documentOptions.AddSchemaTransformer(
            (schema, _, _) =>
            {
                if (schema.Properties is null)
                {
                    return Task.CompletedTask;
                }

                foreach (var property in schema.Properties)
                {
                    if (schema.Required?.Contains(property.Key) != true)
                    {
                        property.Value.Nullable = false;
                    }
                }

                return Task.CompletedTask;
            }
        );
    }
}

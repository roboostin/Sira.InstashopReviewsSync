using API.Filters;
using Serilog;

namespace API.Extensions;

public static class SerilogExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration)
            .Enrich.With(new TruncateAllStringPropertiesEnricher(5000)));
    }

    public static void EnableSerilog(this IApplicationBuilder app)
    {
    }
}
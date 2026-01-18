using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace API.Extensions;

public static class ResponseCompressionExtensions
{
    public static void ConfigureResponseCompression(this IServiceCollection services, IConfiguration configuration)
    {
        // Add response compression services
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true; // Enable compression for HTTPS
            options.Providers.Add<GzipCompressionProvider>(); // Use Gzip
            options.Providers.Add<BrotliCompressionProvider>(); // Use Brotli
        });

// Configure compression options
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });
        
    }

    public static void EnableResponseCompression(this IApplicationBuilder app)
    {
        // Enable response compression middleware
        app.UseResponseCompression();
    }
}
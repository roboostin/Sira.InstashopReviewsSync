using API.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Extensions;

public static class FluentApiExtensions
{
    public static PropertyBuilder<string?> HasEncryptedString(this PropertyBuilder<string?> builder)
        => builder.HasConversion(
            new StringEncryptionConverter(),
            new StringEncryptionComparer()
        );
}


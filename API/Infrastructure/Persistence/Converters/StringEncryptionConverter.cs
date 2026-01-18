using System.Text.RegularExpressions;
using API.Shared.Helpers;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Infrastructure.Persistence.Converters;

public class StringEncryptionConverter(ConverterMappingHints? mappingHints = null) : ValueConverter<string?, string?>(
    plain => plain == null
        ? null
        : SecurityHelper.EncryptValue(plain),
    cipher => cipher == null
        ? null
        : SecurityHelper.DecryptValue(cipher),
    mappingHints);


public class StringEncryptionComparer() : ValueComparer<string?>(
    (l, r) => IsCipher(l) && IsCipher(r)
        ? SecurityHelper.DecryptValue(l!) == SecurityHelper.DecryptValue(r!)
        : l == r, 
    v => IsCipher(v)
        ? SecurityHelper.DecryptValue(v!).GetHashCode()
        : (v ?? string.Empty).GetHashCode(), 
    v => v) 
{
    private static bool IsCipher(string? s)
        => !string.IsNullOrEmpty(s)
           && s.Length % 4 == 0
           && Regex.IsMatch(s, @"^[A-Za-z0-9\+/=]+$");
}

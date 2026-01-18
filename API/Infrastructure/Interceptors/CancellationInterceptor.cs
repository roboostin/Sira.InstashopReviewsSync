using API.Application.Interfaces;
using API.Helpers;
using API.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace API.Infrastructure.Interceptors;
public sealed class CancellationInterceptor : DbCommandInterceptor
{
    private readonly ICancellationTokenProvider _cancellationTokenProvider;
    private CancellationToken commandTimeoutCancellationToken;
    TimeZoneInfo _timeZone => _cancellationTokenProvider?.GetTimeZone() ?? GetEgyptTimeZone();
    CancellationToken GetCommandTimeoutCancellationToken() => new CancellationTokenSource(TimeSpan.FromSeconds(ConfigurationHelper.GetDBCommandTimeOut())).Token;

    public CancellationInterceptor(
        ICancellationTokenProvider provideCancellationToken)
    {
        _cancellationTokenProvider = provideCancellationToken;

    }
    private static TimeZoneInfo GetEgyptTimeZone()
    {
        try
        {
            //  Windows
            return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            // Linux/macOS
            return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        }
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        ConvertDateFiltersToUtc(command);

        var reader = command.ExecuteReader();

        // Skip conversion for EF internal ops
        if (IsInternalEntityFrameworkOperation(command, eventData))
            return InterceptionResult<DbDataReader>.SuppressWithResult(reader);


        var wrappedReader = new UtcToLocalDbDataReader(reader, _timeZone);

        return InterceptionResult<DbDataReader>.SuppressWithResult(wrappedReader);
    }


    //public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
    //   DbCommand command,
    //   CommandEventData eventData,
    //   InterceptionResult<DbDataReader> result,
    //   CancellationToken cancellationToken = default)
    //{
    //    ConvertDateFiltersToUtc(command);

    //    if (_cancellationTokenProvider is null)
    //    {
    //        await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    //    }
    //    else
    //    {
    //        commandTimeoutCancellationToken = GetCommandTimeoutCancellationToken();
    //        cancellationToken = _cancellationTokenProvider.GetCancellationToken(commandTimeoutCancellationToken);
    //        cancellationToken.Register(command.Cancel);

    //        await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    //    }

    //    var reader = await command.ExecuteReaderAsync(cancellationToken);
    //    var wrappedReader = new UtcToLocalDbDataReader(reader, _timeZone);
    //    return InterceptionResult<DbDataReader>.SuppressWithResult(wrappedReader);
    //}

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        ConvertDateFiltersToUtc(command);

        if (_cancellationTokenProvider is null)
        {
            await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
        else
        {
            commandTimeoutCancellationToken = GetCommandTimeoutCancellationToken();
            cancellationToken = _cancellationTokenProvider.GetCancellationToken(commandTimeoutCancellationToken);
            cancellationToken.Register(command.Cancel);

            await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        var reader = await command.ExecuteReaderAsync(cancellationToken);

        // Only wrap the reader if this is not an Entity Framework internal operation
        // Check if this is likely a concurrency check or internal EF operation
        bool isInternalOperation = IsInternalEntityFrameworkOperation(command, eventData);

        if (isInternalOperation)
        {
            //    // Return the original reader for internal operations to avoid casting issues
            return InterceptionResult<DbDataReader>.SuppressWithResult(reader);
    }
        else
        {
        var wrappedReader = new UtcToLocalDbDataReader(reader, _timeZone);
        return InterceptionResult<DbDataReader>.SuppressWithResult(wrappedReader);
    }
    }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        ConvertDateFiltersToUtc(command);

        return base.NonQueryExecuting(command, eventData, result);
    }


    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertDateFiltersToUtc(command);

        commandTimeoutCancellationToken = GetCommandTimeoutCancellationToken();

        cancellationToken = _cancellationTokenProvider.GetCancellationToken(commandTimeoutCancellationToken);
        cancellationToken.Register(command.Cancel);

        return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        //ConvertDateFiltersToUtc(command);

        return base.ScalarExecuting(command, eventData, result);
    }

    public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
    CommandEventData eventData,
    InterceptionResult<object> result,
    CancellationToken cancellationToken = default)
    {
        //ConvertDateFiltersToUtc(command);

        cancellationToken = _cancellationTokenProvider.GetCancellationToken(commandTimeoutCancellationToken);
        cancellationToken.Register(command.Cancel);

        return await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void ConvertDateFiltersToUtc(DbCommand command)
    {
        foreach (var param in command.Parameters.Cast<DbParameter>())
        {
            if (param.Value is DateTime dateTime && dateTime.Kind != DateTimeKind.Utc)
            {
                DateTime time = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);

                // Check for invalid time in the target time zone
                //if (_timeZone.IsInvalidTime(time))
                //{
                //    time = time.AddDays(1);
                //}

                param.Value = TimeZoneInfo.ConvertTimeToUtc(time, _timeZone);
            }
        }
    }


    private bool IsInternalEntityFrameworkOperation(DbCommand command, CommandEventData eventData)
    {
        // Check if this is likely a concurrency check operation
        // Entity Framework concurrency checks typically:
        // 1. Select only specific columns (like RowVersion)
        // 2. Have WHERE clauses with primary key and RowVersion
        // 3. Are executed as part of SaveChanges

        if (command.CommandText?.Contains("Returning", StringComparison.OrdinalIgnoreCase) == true ||
            command.CommandText?.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase)==true||
            command.CommandText?.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        //if (command.CommandText?.Contains("RowVersion") == true)
        //{
        //    // This is likely a concurrency check
        //    return true;
        //}
 
        // Check if this is a simple SELECT with WHERE clause (likely internal EF operation)
        //if (command.CommandText?.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) == true)
        //{
        //    var parameters = command.Parameters.Cast<DbParameter>().ToList();

        //    // If it's a SELECT with only a few parameters and likely primary key lookup
        //    if (parameters.Count <= 2 && command.CommandText.Contains("WHERE"))
        //    {
        //        // Check if it's selecting from a table that has RowVersion (concurrency check)
        //        var entityTypes = eventData.Context?.Model?.GetEntityTypes();
        //        if (entityTypes != null)
        //        {
        //            foreach (var entityType in entityTypes)
        //            {
        //                var tableName = entityType.GetTableName();
        //                if (tableName != null && command.CommandText.Contains(tableName))
        //                {
        //                    var rowVersionProperty = entityType.GetProperties()
        //                        .FirstOrDefault(p => p.Name == "RowVersion");
        //                    if (rowVersionProperty != null)
        //                    {
        //                        return true; // This is likely a concurrency check
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        return false;
    }

    //private bool IsInternalEntityFrameworkOperation(DbCommand command, CommandEventData eventData)
    //{
    //    // Pattern 1: Explicit concurrency check during SaveChanges
    //    // These queries typically:
    //    // - Select only RowVersion (and maybe 1-2 other fields)
    //    // - Have WHERE clause with primary key AND RowVersion
    //    // - Are single row lookups

    //    var commandText = command.CommandText?.Trim() ?? "";

    //    // Check if this looks like a concurrency check query
    //    if (commandText.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
    //    {
    //        // Count the selected columns (rough approximation)
    //        var selectPart = GetSelectClause(commandText);
    //        var columnCount = CountSelectedColumns(selectPart);

    //        // Check if it's selecting very few columns and has RowVersion in WHERE clause
    //        if (columnCount <= 3 &&
    //            commandText.Contains("WHERE", StringComparison.OrdinalIgnoreCase) &&
    //            commandText.Contains("RowVersion", StringComparison.OrdinalIgnoreCase))
    //        {
    //            // Check if RowVersion is in the WHERE clause (not just in SELECT)
    //            var whereIndex = commandText.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
    //            if (whereIndex > 0)
    //            {
    //                var whereClause = commandText.Substring(whereIndex);
    //                if (whereClause.Contains("RowVersion"))
    //                {
    //                    // This is likely a concurrency check
    //                    return true;
    //                }
    //            }
    //        }
    //    }

    //    // Pattern 2: Check if this is a generated identity/output clause query
    //    if (commandText.Contains("OUTPUT INSERTED", StringComparison.OrdinalIgnoreCase) ||
    //        commandText.Contains("SCOPE_IDENTITY()", StringComparison.OrdinalIgnoreCase) ||
    //        commandText.Contains("@@ROWCOUNT", StringComparison.OrdinalIgnoreCase))
    //    {
    //        return true;
    //    }

    //    // Pattern 3: EF Migration or metadata queries
    //    if (commandText.Contains("__EFMigrationsHistory", StringComparison.OrdinalIgnoreCase) ||
    //        commandText.Contains("INFORMATION_SCHEMA", StringComparison.OrdinalIgnoreCase))
    //    {
    //        return true;
    //    }

    //    return false;
    //}

    //private string GetSelectClause(string commandText)
    //{
    //    var selectIndex = commandText.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
    //    var fromIndex = commandText.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);

    //    if (selectIndex >= 0 && fromIndex > selectIndex)
    //    {
    //        return commandText.Substring(selectIndex + 6, fromIndex - selectIndex - 6);
    //    }

    //    return "";
    //}

    //private int CountSelectedColumns(string selectClause)
    //{
    //    if (string.IsNullOrWhiteSpace(selectClause))
    //        return 0;

    //    // Remove TOP clause if present
    //    selectClause = System.Text.RegularExpressions.Regex.Replace(
    //        selectClause,
    //        @"TOP\s*KATEX_INLINE_OPEN\s*\d+\s*KATEX_INLINE_CLOSE",
    //        "",
    //        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    //    selectClause = System.Text.RegularExpressions.Regex.Replace(
    //        selectClause,
    //        @"TOP\s+\d+",
    //        "",
    //        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    //    // Very rough count - just count commas + 1
    //    // This won't be perfect but good enough for our heuristic
    //    return selectClause.Count(c => c == ',') + 1;
    //}
}

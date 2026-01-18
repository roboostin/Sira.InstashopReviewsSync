using System;
using DayOfWeekEnum = API.Domain.Enums.DayOfWeek;

namespace API.Shared.Helpers;

/// <summary>
/// Helper class for DayOfWeek operations.
/// </summary>
public static class DayOfWeekHelper
{
    /// <summary>
    /// Gets the name of the day from a DayOfWeek enum value.
    /// </summary>
    /// <param name="day">The DayOfWeek value.</param>
    /// <returns>The name of the day (e.g., "Monday", "Tuesday", etc.).</returns>
    public static string GetDayName(DayOfWeek day)
    {
        return day.ToString();
    }

    /// <summary>
    /// Gets the name of the day from a domain DayOfWeek enum value.
    /// </summary>
    /// <param name="day">The domain DayOfWeek enum value.</param>
    /// <returns>The name of the day (e.g., "Monday", "Tuesday", etc.).</returns>
    public static string GetDayName(DayOfWeekEnum day)
    {
        return day.ToString();
    }
}


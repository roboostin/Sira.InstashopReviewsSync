using API.Domain.Enums;
using API.Shared.Helpers;

namespace API.Shared.Models;

public class UserState
{
    public ApplicationRole RoleID { get; set; }
    public long UserID { get; set; }
    public long CompanyID { get; set; }
    public string Username { get; set; } = "System";
    public int UtcOffset { get; set; }

    public void SetUserData(long? userID, ApplicationRole roleID, long? companyID, string? userName, string TZ = null)
    {
        UserID = userID ?? 0;
        RoleID = roleID;
        Username = userName ?? "System";
        CompanyID = companyID ?? 0;
        UtcOffset = GetUtcOffset(TZ);
    }
    private int GetUtcOffset(string TZ)
    {
        TimeZoneInfo timeZone = null;

        if (TZ != null && TZ != "")
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(TZ);

        if (timeZone == null)
        {
            try
            {
                // Windows
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Linux/macOS
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
            }
        }
        return (int)timeZone.GetUtcOffset(DateTime.UtcNow).TotalHours;
    }
}


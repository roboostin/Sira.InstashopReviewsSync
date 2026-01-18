using API.Application.Interfaces;

namespace API.Infrastructure.Persistence.DbContexts;

public class CancellationTokenProvider : ICancellationTokenProvider
{
    private CancellationToken _token;
    private TimeZoneInfo _timeZone;

    TimeZoneInfo ICancellationTokenProvider.GetTimeZone()
    {
        return _timeZone;
    }

    void ICancellationTokenProvider.CaptureTimeZone(string timeZoneID)
    {
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneID);
    }

    CancellationToken ICancellationTokenProvider.GetCancellationToken(CancellationToken additionalCancellationToken = default)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(_token, additionalCancellationToken).Token;
    }

    void ICancellationTokenProvider.CaptureToken(CancellationToken token)
    {
        _token = token;
    }
}
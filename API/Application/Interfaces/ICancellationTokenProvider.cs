namespace API.Application.Interfaces;

public interface ICancellationTokenProvider
{
    void CaptureToken(CancellationToken token);
    CancellationToken GetCancellationToken(CancellationToken additionalCancellationToken = default);
    void CaptureTimeZone(string timeZoneID);
    TimeZoneInfo GetTimeZone();

}
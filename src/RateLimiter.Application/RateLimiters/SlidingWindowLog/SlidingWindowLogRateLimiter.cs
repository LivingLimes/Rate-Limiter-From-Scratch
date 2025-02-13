namespace RateLimiter.Application.RateLimiters.SlidingWindowLog;

public class SlidingWindowLogRateLimiter : IRateLimiter
{
    private int Limit;
    private int PeriodInSeconds;
    private List<DateTimeOffset> _log;

    public SlidingWindowLogRateLimiter(int limit, int periodInSeconds)
    {
        Limit = limit;
        PeriodInSeconds = periodInSeconds;
        _log = new List<DateTimeOffset>();
    }

    public bool RateLimit()
    {
        RemoveObsoleteLogs();
        if (_log.Count < Limit)
        {
            _log.Add(DateTimeOffset.UtcNow);
            return true;
        }

        return false;
    }

    private void RemoveObsoleteLogs()
    {
        _log = _log.Where(l => l > DateTimeOffset.UtcNow.AddSeconds(-PeriodInSeconds)).ToList();
    }
}
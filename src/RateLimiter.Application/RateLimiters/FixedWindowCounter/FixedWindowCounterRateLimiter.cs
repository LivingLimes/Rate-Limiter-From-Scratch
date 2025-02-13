namespace RateLimiter.Application.RateLimiters.FixedWindowCounter;

public class FixedWindowCounterRateLimiter : IRateLimiter
{
    private int Limit;
    private int PeriodInSeconds;
    private DateTimeOffset LastReset;
    private int Count;

    public FixedWindowCounterRateLimiter(int limit, int periodInSeconds)
    {
        Limit = limit;
        PeriodInSeconds = periodInSeconds;
        LastReset = DateTimeOffset.UtcNow;
        Count = Limit;
    }

    public bool RateLimit()
    {
        Refill();
        if (Count > 0)
        {
            Count--;
            return true;
        }

        return false;
    }

    private void Refill()
    {
        var secondsSinceLastReset = (DateTimeOffset.UtcNow - LastReset).TotalSeconds;
        if (secondsSinceLastReset >= PeriodInSeconds)
        {
            Count = Limit;
            LastReset = DateTimeOffset.UtcNow;
        }
    }
}

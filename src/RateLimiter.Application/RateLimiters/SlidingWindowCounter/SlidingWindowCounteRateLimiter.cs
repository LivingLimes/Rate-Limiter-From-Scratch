namespace RateLimiter.Application.RateLimiters.SlidingWindowCounter;

public class SlidingWindowCounterRateLimiter : IRateLimiter
{
    private int Limit;
    private int WindowSizeInSeconds;
    private int Count;
    private DateTimeOffset LastRateLimitCheck;

    public SlidingWindowCounterRateLimiter(int limit, int windowSizeInSeconds)
    {
        Limit = limit;
        WindowSizeInSeconds = windowSizeInSeconds;
        Count = 0;
        LastRateLimitCheck = DateTimeOffset.UtcNow;
    }

    public bool RateLimit()
    {
        var now = DateTimeOffset.UtcNow;
        
        ResetCount(now);

        LastRateLimitCheck = now;

        if (Count < Limit)
        {
            Count++;
            return true;
        }

        return false;
    }

    private void ResetCount(DateTimeOffset now)
    {
        var requestsPerSecondToRecover = (double)Limit / WindowSizeInSeconds;
        var requestsToRecover = now.Subtract(LastRateLimitCheck).TotalSeconds * requestsPerSecondToRecover;
        Count = Math.Max(0, Count - (int)requestsToRecover);
    }
}

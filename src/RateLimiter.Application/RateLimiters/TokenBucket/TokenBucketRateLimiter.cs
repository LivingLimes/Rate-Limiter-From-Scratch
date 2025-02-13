namespace RateLimiter.Application.RateLimiters.TokenBucket;

public class TokenBucketRateLimiter : IRateLimiter
{
    private int Limit;
    private int PeriodInSeconds;
    private DateTimeOffset LastRefill;
    private int Tokens;

    public TokenBucketRateLimiter(int limit, int periodInSeconds)
    {
        Limit = limit;
        PeriodInSeconds = periodInSeconds;
        Tokens = Limit;
        LastRefill = DateTimeOffset.UtcNow;
    }


    public bool RateLimit()
    {
        Refill();

        if (Tokens > 0)
        {
            Tokens--;
            return true;
        }


        return false;
    }
    
    private void Refill()
    {
        var periodsElapsed = (DateTimeOffset.UtcNow - LastRefill).TotalSeconds / PeriodInSeconds;
        var tokensToAdd = (int)(periodsElapsed * Limit);
        Tokens = (int)Math.Min(Tokens + tokensToAdd, Limit);
        LastRefill = DateTimeOffset.UtcNow;
    }
}

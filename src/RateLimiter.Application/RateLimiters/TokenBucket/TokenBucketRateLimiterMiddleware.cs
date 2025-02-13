namespace RateLimiter.Application.RateLimiters.TokenBucket;

using Microsoft.Extensions.Options;

public class TokenBucketRateLimiterMiddleware : IMiddleware
{
    private readonly UsagePlanConfig _config;
    private Dictionary<string, TokenBucketRateLimiter> _tokens;
    private const string AlgorithmName = "TokenBucket";

    public TokenBucketRateLimiterMiddleware(IOptions<UsagePlanConfig> config)
    {
        _config = config.Value;
        _tokens = new Dictionary<string, TokenBucketRateLimiter>();
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path == "/not-rate-limited")
        {
            await next(context);
            return;
        }

        if (_config.Algorithm != AlgorithmName)
        {
            await next(context);
            return;
        }

        var ip = RequesterGetter.Get(context);
        if (ip == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Unable to determine requester. Request rejected.");
            return;
        }

        if (!_tokens.ContainsKey(ip))
        {
            _tokens.Add(ip, new TokenBucketRateLimiter(limit: _config.Limit, periodInSeconds: _config.PeriodInSeconds));
        }

        var canAcceptRequest = _tokens[ip].RateLimit();
        if (!canAcceptRequest)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await next(context);
    }
}

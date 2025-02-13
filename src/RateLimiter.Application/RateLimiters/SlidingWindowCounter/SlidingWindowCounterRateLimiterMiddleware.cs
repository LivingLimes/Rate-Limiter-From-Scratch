namespace RateLimiter.Application.RateLimiters.SlidingWindowCounter;

using Microsoft.Extensions.Options;

public class SlidingWindowCounterRateLimiterMiddleware : IMiddleware
{
    private readonly UsagePlanConfig _config;
    private Dictionary<string, SlidingWindowCounterRateLimiter> _counters;
    private const string AlgorithmName = "SlidingWindowCounter";
    public SlidingWindowCounterRateLimiterMiddleware(IOptions<UsagePlanConfig>  config)
    {
        _config = config.Value;
        _counters = new Dictionary<string, SlidingWindowCounterRateLimiter>();
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

        if (!_counters.ContainsKey(ip))
        {
            _counters.Add(ip, new SlidingWindowCounterRateLimiter(limit: _config.Limit, windowSizeInSeconds: _config.PeriodInSeconds));
        }

        var canAcceptRequest = _counters[ip].RateLimit();
        Console.WriteLine("CanAcceptRequest: " + canAcceptRequest);
        if (!canAcceptRequest)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await next(context);
    }
}
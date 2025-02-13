namespace RateLimiter.Application.RateLimiters.FixedWindowCounter;

using Microsoft.Extensions.Options;

public class FixedWindowCounterRateLimiterMiddleware: IMiddleware
{
    private readonly UsagePlanConfig _config;
    private Dictionary<string, FixedWindowCounterRateLimiter> _counters;
    private const string AlgorithmName = "FixedWindowCounter";

    public FixedWindowCounterRateLimiterMiddleware(IOptions<UsagePlanConfig> config)
    {
        _config = config.Value;
        _counters = new Dictionary<string, FixedWindowCounterRateLimiter>();
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
            _counters.Add(ip, new FixedWindowCounterRateLimiter(limit: _config.Limit, periodInSeconds: _config.PeriodInSeconds));
        }

        var canAcceptRequest = _counters[ip].RateLimit();
        if (!canAcceptRequest)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await next(context);
    }

}

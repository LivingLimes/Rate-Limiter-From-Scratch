namespace RateLimiter.Application.RateLimiters.SlidingWindowLog;

using Microsoft.Extensions.Options;

public class SlidingWindowLogRateLimiterMiddleware : IMiddleware
{
    private readonly UsagePlanConfig _config;
    private Dictionary<string, SlidingWindowLogRateLimiter> _logs;
    private const string AlgorithmName = "SlidingWindowLog";

    public SlidingWindowLogRateLimiterMiddleware(IOptions<UsagePlanConfig> config)
    {
        _config = config.Value;
        _logs = new Dictionary<string, SlidingWindowLogRateLimiter>();
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

        if (!_logs.ContainsKey(ip))
        {
            _logs.Add(ip, new SlidingWindowLogRateLimiter(limit: _config.Limit, periodInSeconds: _config.PeriodInSeconds));
        }

        var canAcceptRequest = _logs[ip].RateLimit();
        if (!canAcceptRequest)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await next(context);
    }
}

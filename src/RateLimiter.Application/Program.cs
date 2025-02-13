using RateLimiter.Application.RateLimiters.FixedWindowCounter;
using RateLimiter.Application.RateLimiters.SlidingWindowCounter;
using RateLimiter.Application.RateLimiters.SlidingWindowLog;
using RateLimiter.Application.RateLimiters.TokenBucket;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<UsagePlanConfig>(
    builder.Configuration.GetSection(UsagePlanConfig.Key));

// We use singleton as the middlewares contains state that tracks the rate limiting. This needs to be shared across requests so that the rate limiter knows to accept or reject the request.
builder.Services.AddSingleton<FixedWindowCounterRateLimiterMiddleware>();
builder.Services.AddSingleton<SlidingWindowCounterRateLimiterMiddleware>();
builder.Services.AddSingleton<SlidingWindowLogRateLimiterMiddleware>();
builder.Services.AddSingleton<TokenBucketRateLimiterMiddleware>();

var app = builder.Build();

app.UseMiddleware<FixedWindowCounterRateLimiterMiddleware>();
app.UseMiddleware<SlidingWindowCounterRateLimiterMiddleware>();
app.UseMiddleware<SlidingWindowLogRateLimiterMiddleware>();
app.UseMiddleware<TokenBucketRateLimiterMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/rate-limited", () =>
{
    return 1;
});

app.MapGet("/not-rate-limited", () =>
{
    return 1;
});

app.Run();

public partial class Program { }
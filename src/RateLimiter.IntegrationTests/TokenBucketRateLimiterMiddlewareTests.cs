namespace RateLimiter.IntegrationTests;

using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

public class TokenBucketRateLimiterMiddlewareTests
{
    [Fact]
    public async Task TokenBucketRateLimiterMiddleware_WhenRateLimitExceeded_Returns429ResponseCode()
    {
        var requestLimit = 1;
        var requestLimitPlusOne = requestLimit + 1;
        var periodInSeconds = 2;
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.Configure<UsagePlanConfig>(opts =>
                {
                    opts.Algorithm = "TokenBucket";
                    opts.Limit = requestLimit;
                    opts.PeriodInSeconds = periodInSeconds;
                });
            });
        });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");

        for (int i = 0; i < requestLimitPlusOne; i++)
        {
            var response = await client.GetAsync("/not-rate-limited");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        for (int i = 0; i < requestLimit; i++)
        {
            var response = await client.GetAsync("/rate-limited");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        var failedResponse = await client.GetAsync("/rate-limited");
        Assert.Equal(HttpStatusCode.TooManyRequests, failedResponse.StatusCode);

        Thread.Sleep(periodInSeconds* 1000);

        var afterRateLimitPeriodResponse = await client.GetAsync("/rate-limited");
        Assert.Equal(HttpStatusCode.OK, afterRateLimitPeriodResponse.StatusCode);
    }
}
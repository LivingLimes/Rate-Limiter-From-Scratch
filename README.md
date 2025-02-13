# Rate Limiter From Scratch

## Pre requisites
- [.Net 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0): This is required to build, develop, compile and run a .Net application.

## To run
```sh
# Run at root level
dotnet run --project src/RateLimiter.Application
```

To modify the rate limiting algorithm, you can change the algorithm in `UsagePlan:Algorithm` inside `appsettings.json`. Valid values are: 
- `FixedWindowCounter`
- `SlidingWindowCounter`
- `SlidingWindowLog`
- `TokenBucket`

The limit and period can also be changed from `appsettings.json`.

## To test
```sh
# Run at root level
dotnet test
```

So far only middleware tests have been implemented.

## What is a Rate Limiter?
A rate limiter is a tool that limits the rate of requests from a requester to a resource. 

## Considerations when implementing a Rate Limiter
### What constitutes a requester?
Note: identifiers can be combined such ad IP address + user id. Here are some examples:
  - IP address
  - User id
  - Api Key
  - Browser fingerprint

### What constitutes a resource?

Typically a resource is an api endpoint. However, rate limiting can be applied both on the client side and server side. It is applied on the server side in this example to protect the api endpoint from abuse and it is applied on the client side to respect the api endpoint's rate limit quota.

### How should you store the rate limit state?
In order to rate limit, we must store data about how many requests have been made by a requester and this data needs to be shared across requests. In this code, I have stored it in memory and shared it across requests using a singleton during dependency injection, but other options include database, cache and file etc..

### Silently dropping requests
If you know a requester is abusing the api, you can silently drop the requests, by simply returning a 200 OK response. For legitimate requesters, you would want to return a 429 error response so they know how to better work with your api endpoint.

### Miscellaneous considerations
- Do you need a way to temporarily disable rate limiting?
- Do you need a way for certain users to be exempt from rate limiting?

## Rate Limiting Strategies
### Token Bucket
- Algorithm:
1. Each requester starts off with a bucket full of tokens.
2. Each request consumes one token.
3. Tokens are refilled on each request. Tokens are added to the bucket based on the time that has passed since the last request.
4. If the bucket is empty, the request is rejected, otherwise a token is removed and the request is accepted.
- Use case: Simple algorithm that can be used when occasional bursts are acceptable. It allows bursts as requesters can use all their tokens at once and they will immediately begin refilling.
- Note: If requests are made in rapid succession, the bucket may not replenish as many tokens as the amount of time that has passed may not have been enough to produce a token.

### Sliding Window Log
- Algorithm:
1. Record the timestamp of each request in a log.
2. Use the log timestamps to determine if the requester has made more requests than the limit in the time period.
3. If the requester has more logs than the limit, the request is rejected, otherwise the request is accepted.
- Use case: Used to create a smooth rate limit that does not allow for bursts. This algorithm requires a lot of memory usage to store the logs.

### Fixed Window Counter
- Algorithm:
1. Each requester gets a certain number of requests per period and this counter is reset every period.
2. If, in a period, the requester has made more requests than the limit, the request is rejected, otherwise the request is accepted.
- Use case: When you value simplicity and ease of implementation. It allows for bursts.

### Sliding Window Counter
- Algorithm:
1. Each requester gets a certain number of requests per window and a counter tracks the number of requests made in the current window.
2. The counter is decremented in each request based on the amount of elapsed time since the last request.
3. If the counter is greater than the limit, the request is rejected, otherwise the request is accepted.
- Use case: Used to create a smooth rate limit that does not allow for bursts, but requires less memory than the sliding window log.
- Note: Within the rate limit period, you can have multiple windows to increase the accuracy of the rate limit as a basic sliding window algorithm estimates how much to decrement the counter based on the window size.

### Leaky Bucket
- Algorithm:
1. Each requester has a bucket with a fixed capacity.
2. Requesters make requests, which are added to the bucket.
3. If the bucket is full, the request is rejected, otherwise the request is accepted and the bucket's level is incremented.
4. The bucket leaks at a fairly constant rate, allowing new requests to be accepted over time.
- Use case: Useful for processing of requests at a steady rate. It prevents bursts of traffic.

### Quota based
- Algorithm:
1. Each requester has a quota of requests they can make within a specific time period.
2. The quota is decremented with each request.
3. If the quota is exhausted, the request is rejected, otherwise the request is accepted.
4. The quota is reset after the time period elapses.
- Use case: Useful for scenarios where you want to enforce strict limits on the number of requests a user can make over a longer period, such as daily or monthly limits.

### Dynamic
- Algorithm:
1. The rate limit is adjusted dynamically based on various factors such as server load, time of day, or user behavior. The adjustment can be handled by a rules based or a machine learning based system.
2. Requests are evaluated against the current dynamic rate limit.
3. If the request exceeds the dynamic rate limit, it is rejected, otherwise it is accepted.
- Use case: Suitable for systems that need to adapt to changing conditions and maintain optimal performance. It allows for more flexible and intelligent rate limiting based on real-time data.

### With queues
- Algorithm:
1. Requests are placed in a queue when the rate limit is reached.
2. The queue processes requests at a steady rate, ensuring that the system is not overwhelmed.
3. If the queue is full, new requests are rejected, otherwise they are added to the queue.
4. Requests are processed in the order they were received.
- Use case: Useful for handling bursts of traffic by queuing requests and processing them at a controlled rate. It ensures that all requests are eventually processed without overwhelming the system, unless the queue gets full.
public static class RequesterGetter
{
    public static string? Get(HttpContext context)
    {
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
        {
             ip = context.Connection.RemoteIpAddress?.ToString();
        }

        return ip;
    }
}
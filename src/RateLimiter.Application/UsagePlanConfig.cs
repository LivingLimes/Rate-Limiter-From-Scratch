public class UsagePlanConfig
{
    public const string Key = "UsagePlan";
    public string Algorithm { get; set; } = string.Empty;
    public int Limit { get; set; } = 0;
    public int PeriodInSeconds { get; set; } = 0;
} 
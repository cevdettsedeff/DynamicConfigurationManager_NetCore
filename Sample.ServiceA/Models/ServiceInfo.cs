namespace Sample.ServiceA.Models;

public class ServiceInfo
{
    public string ServiceName { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public int MaxItemCount { get; set; }
    public double ConnectionTimeout { get; set; }
    public bool IsFeatureEnabled { get; set; }
    public DateTime LoadedAt { get; set; }
}
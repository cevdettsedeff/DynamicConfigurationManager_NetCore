// ConfigurationReader.Domain/Entities/ConfigurationItem.cs
using ConfigurationReader.Domain.Enums;

namespace ConfigurationReader.Domain.Entities;

public class ConfigurationItem
{
    // ✅ Private constructor for EF Core
    private ConfigurationItem()
    {
    }

    // ✅ Factory method (DDD pattern)
    public static ConfigurationItem Create(
        string name,
        ConfigurationType type,
        string value,
        string applicationName,
        bool isActive = true)
    {
        var now = DateTime.UtcNow;

        return new ConfigurationItem
        {
            Name = name,
            Type = type,
            Value = value,
            ApplicationName = applicationName,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ConfigurationType Type { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public string ApplicationName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public byte[]? RowVersion { get; private set; }

    public void UpdateValue(string value)
    {
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
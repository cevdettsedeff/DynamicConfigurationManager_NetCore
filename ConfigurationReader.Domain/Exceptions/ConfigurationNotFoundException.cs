namespace ConfigurationReader.Domain.Exceptions;

public class ConfigurationNotFoundException : Exception
{
    public ConfigurationNotFoundException(string key)
        : base($"Configuration with key '{key}' was not found.")
    {
    }

    public ConfigurationNotFoundException(string applicationName, string key)
        : base($"Configuration '{key}' not found for application '{applicationName}'.")
    {
    }
}
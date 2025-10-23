namespace ConfigurationReader.AdminPanel.Models;

public class CreateConfigurationDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string Value { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string ApplicationName { get; set; } = string.Empty;
}
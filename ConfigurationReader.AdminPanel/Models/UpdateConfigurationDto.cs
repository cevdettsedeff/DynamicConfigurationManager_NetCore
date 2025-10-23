namespace ConfigurationReader.AdminPanel.Models;

public class UpdateConfigurationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
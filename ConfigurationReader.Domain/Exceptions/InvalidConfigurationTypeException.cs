namespace ConfigurationReader.Domain.Exceptions;

public class InvalidConfigurationTypeException : Exception
{
    public InvalidConfigurationTypeException(string value, string expectedType)
        : base($"Cannot convert value '{value}' to type '{expectedType}'.")
    {
    }
}
using ConfigurationReader.Domain.Exceptions;
using System.Globalization;

namespace ConfigurationReader.Infrastructure.Services;

public static class TypeConverterService
{
    public static T Convert<T>(string value)
    {
        var targetType = typeof(T);

        try
        {
            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (string.IsNullOrWhiteSpace(value) && targetType.IsGenericType &&
                targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return default!;
            }

            if (underlyingType == typeof(string))
            {
                return (T)(object)value;
            }

            if (underlyingType == typeof(int))
            {
                return (T)(object)int.Parse(value, CultureInfo.InvariantCulture);
            }

            if (underlyingType == typeof(bool))
            {
                return (T)(object)bool.Parse(value);
            }

            if (underlyingType == typeof(double))
            {
                return (T)(object)double.Parse(value, CultureInfo.InvariantCulture);
            }

            if (underlyingType == typeof(decimal))
            {
                return (T)(object)decimal.Parse(value, CultureInfo.InvariantCulture);
            }

            if (underlyingType == typeof(long))
            {
                return (T)(object)long.Parse(value, CultureInfo.InvariantCulture);
            }

            if (underlyingType == typeof(float))
            {
                return (T)(object)float.Parse(value, CultureInfo.InvariantCulture);
            }

            if (underlyingType == typeof(DateTime))
            {
                return (T)(object)DateTime.Parse(value, CultureInfo.InvariantCulture);
            }

            throw new InvalidConfigurationTypeException(value, targetType.Name);
        }
        catch (Exception ex) when (ex is not InvalidConfigurationTypeException)
        {
            throw new InvalidConfigurationTypeException(value, targetType.Name);
        }
    }
}
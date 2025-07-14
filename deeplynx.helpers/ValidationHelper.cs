using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace deeplynx.helpers;

public class ValidationHelper
{
    public static void ValidateModel<T>(T model)
    {
        var valContext = new ValidationContext(model, null, null);
        Validator.ValidateObject(model, valContext, validateAllProperties: true);
        ValidateLongProperties(model);
    }
    private static void ValidateLongProperties<T>(T model)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(long))
            {
                var value = (long)property.GetValue(model);
                if (value < 1 || value > long.MaxValue)
                {
                    throw new ValidationException($"{property.Name} must be between 1 and {long.MaxValue}.");
                }
            }
        }
    }
}
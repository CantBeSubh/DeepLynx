using System.ComponentModel.DataAnnotations;

namespace deeplynx.helpers;

public class ValidationHelper
{
    public static void ValidateModel<T>(T model)
    {
        var valContext = new ValidationContext(model, null, null);
        Validator.ValidateObject(model, valContext, validateAllProperties: true);
    }
}
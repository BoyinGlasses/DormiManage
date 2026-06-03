using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.Validation;

public static class RequestValidator
{
    public static ValidationResult Validate(object request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = new ValidationContext(request);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

        return isValid
            ? ValidationResult.Success()
            : new ValidationResult(results.Select(x => x.ErrorMessage ?? "Invalid value."));
    }

    public static void ValidateAndThrow(object request)
    {
        var result = Validate(request);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}

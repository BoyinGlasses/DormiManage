namespace DormitoryManagement.Application.Validation;

public sealed class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base("Request validation failed.")
    {
        Errors = errors.ToArray();
    }
}

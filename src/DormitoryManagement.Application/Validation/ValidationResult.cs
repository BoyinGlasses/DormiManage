namespace DormitoryManagement.Application.Validation;

public sealed class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public IReadOnlyList<string> Errors { get; }

    public ValidationResult(IEnumerable<string> errors)
    {
        Errors = errors.ToArray();
    }

    public static ValidationResult Success() => new(Array.Empty<string>());
}

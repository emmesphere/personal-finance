namespace PersonalFinance.BuildingBlocks.Results;

public sealed record ValidationError : ResultError
{
    public ValidationError(ResultError[] errors)
        : base(
            "Validation.General",
            "One or more validation errors occurred",
            ErrorType.Validation)
    {
        Errors = (errors ?? Enumerable.Empty<ResultError>()).ToArray();
    }

    public IReadOnlyList<ResultError> Errors { get; }

    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
}

namespace PersonalFinance.BuildingBlocks.Results;

public record ResultError
{
    public static readonly ResultError None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly ResultError NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

    public ResultError(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }

    public string Description { get; }

    public ErrorType Type { get; }

    public static ResultError Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static ResultError NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static ResultError Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static ResultError Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static ResultError Validation(string code, string description) =>
       new(code, description, ErrorType.Validation);
}

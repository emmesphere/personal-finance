using System.Diagnostics.CodeAnalysis;

namespace PersonalFinance.BuildingBlocks.Results;

public class Result
{
    public Result(bool isSuccess, ResultError error)
    {
        if (isSuccess && error != ResultError.None ||
            !isSuccess && error == ResultError.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public ResultError Error { get; }

    public static Result Success() => new(true, ResultError.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, ResultError.None);

    public static Result Failure(ResultError error) => new(false, error);

    public static Result<TValue> Failure<TValue>(ResultError error) =>
        new(default, false, error);

    public static Result<TValue> ValidationFailure<TValue>(ResultError error) =>
        new(default, false, error);

}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, ResultError error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(ResultError.NullValue);

    

    public Result<TValue> ToResult(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(ResultError.NullValue);
    
}

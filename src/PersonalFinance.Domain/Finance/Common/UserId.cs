using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Extensions;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Domain.Finance.Common;

public sealed class UserId : ValueObject
{
	public Guid Value { get; }

	private UserId() { }

	internal static UserId From(Guid userId) => new(userId);

	protected override IEnumerable<object?> GetEqualityComponents()
	{
		yield return Value;
	}

	public static Result<UserId> Create(Guid userId)
	{
		if (userId.IsEmpty())
			return Result.Failure<UserId>(ResultError.Validation("UserId.Empty", "UserId cannot be empty"));

		return Result.Success(new UserId(userId));

	}

	private UserId(Guid value) => Value = value;

	public override string ToString() => Value.ToString();

}
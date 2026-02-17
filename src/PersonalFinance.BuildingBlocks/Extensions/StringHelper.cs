namespace PersonalFinance.BuildingBlocks.Extensions;

public static class StringHelper
{
	public static bool IsEmpty(this string? value) => string.IsNullOrWhiteSpace(value);
}

public static class GuidHelper
{
	public static bool IsEmpty(this Guid value) => Guid.Empty == value;
}
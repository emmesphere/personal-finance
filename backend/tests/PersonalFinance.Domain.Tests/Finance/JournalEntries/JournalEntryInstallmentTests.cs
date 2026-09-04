using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.JournalEntries;

using Shouldly;

namespace PersonalFinance.Domain.Tests.Finance.JournalEntries;

public sealed class JournalEntryInstallmentTests
{
    private static JournalEntry CreateDraftEntry()
        => JournalEntry.Create(Guid.NewGuid(), UserId.From(Guid.NewGuid()), DateOnly.FromDateTime(DateTime.UtcNow), "test").Value;

    [Fact]
    public void AssignInstallment_ShouldSucceed_WhenDraftAndValid()
    {
        var entry = CreateDraftEntry();

        var result = entry.AssignInstallment(Guid.NewGuid(), 2, 3);

        result.IsSuccess.ShouldBeTrue();
        entry.InstallmentNumber.ShouldBe(2);
        entry.InstallmentTotalCount.ShouldBe(3);
    }

    [Fact]
    public void AssignInstallment_ShouldFail_WhenPlanIdIsEmpty()
    {
        var entry = CreateDraftEntry();

        var result = entry.AssignInstallment(Guid.Empty, 1, 1);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("JournalEntry.InstallmentPlanId.Empty");
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(4, 3)]
    public void AssignInstallment_ShouldFail_WhenNumberOutOfRange(int number, int total)
    {
        var entry = CreateDraftEntry();

        var result = entry.AssignInstallment(Guid.NewGuid(), number, total);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("JournalEntry.InstallmentNumber.Invalid");
    }

    [Fact]
    public void AssignInstallment_ShouldFail_WhenTotalLessThanOne()
    {
        var entry = CreateDraftEntry();

        var result = entry.AssignInstallment(Guid.NewGuid(), 1, 0);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("JournalEntry.InstallmentTotal.Invalid");
    }
}

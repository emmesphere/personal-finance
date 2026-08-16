using PersonalFinance.Domain.Finance.Accounts;

using Shouldly;

namespace PersonalFinance.Domain.Tests.Finance.Accounts;

public sealed class AccountTests
{
    [Theory]
    [InlineData(AccountType.CreditCard)]
    [InlineData(AccountType.Debit)]
    public void Create_ShouldSucceed_WithDueDate_ForCreditCardOrDebit(AccountType type)
    {
        var dueDate = DueDate.Create(10).Value;

        var result = Account.Create(Guid.NewGuid(), "Credit Card", type, dueDate, categoryId: null, DateTime.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        result.Value.DueDate.ShouldBe(dueDate);
    }

    [Theory]
    [InlineData(AccountType.CreditCard)]
    [InlineData(AccountType.Debit)]
    public void Create_ShouldSucceed_WithoutDueDate_ForCreditCardOrDebit(AccountType type)
    {
        var result = Account.Create(Guid.NewGuid(), "Credit Card", type, dueDate: null, categoryId: null, DateTime.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        result.Value.DueDate.ShouldBeNull();
    }

    [Theory]
    [InlineData(AccountType.BankAccount)]
    [InlineData(AccountType.Wallet)]
    [InlineData(AccountType.Benefit)]
    public void Create_ShouldFail_WhenDueDateProvided_ForNonCreditCardOrDebitType(AccountType type)
    {
        var dueDate = DueDate.Create(10).Value;

        var result = Account.Create(Guid.NewGuid(), "My Account", type, dueDate, categoryId: null, DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.DueDate.NotAllowed");
    }

    [Theory]
    [InlineData(AccountType.Income)]
    [InlineData(AccountType.Expense)]
    public void Create_ShouldFail_WhenCategoryIdMissing_ForIncomeOrExpenseType(AccountType type)
    {
        var result = Account.Create(Guid.NewGuid(), "Salary", type, dueDate: null, categoryId: null, DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.CategoryId.Required");
    }

    [Theory]
    [InlineData(AccountType.Income)]
    [InlineData(AccountType.Expense)]
    public void Create_ShouldSucceed_WhenCategoryIdProvided_ForIncomeOrExpenseType(AccountType type)
    {
        var result = Account.Create(Guid.NewGuid(), "Salary", type, dueDate: null, categoryId: Guid.NewGuid(), DateTime.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        result.Value.CategoryId.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(AccountType.BankAccount)]
    [InlineData(AccountType.Wallet)]
    [InlineData(AccountType.Benefit)]
    [InlineData(AccountType.CreditCard)]
    [InlineData(AccountType.Debit)]
    public void Create_ShouldFail_WhenCategoryIdProvided_ForNonIncomeOrExpenseType(AccountType type)
    {
        var result = Account.Create(Guid.NewGuid(), "My Account", type, dueDate: null, categoryId: Guid.NewGuid(), DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.CategoryId.NotAllowed");
    }

    [Fact]
    public void Create_ShouldFail_WhenLedgerIdIsEmpty()
    {
        var result = Account.Create(Guid.Empty, "Checking", AccountType.Wallet, dueDate: null, categoryId: null, DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.Ledger.Empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenNameIsBlank(string name)
    {
        var result = Account.Create(Guid.NewGuid(), name, AccountType.Wallet, dueDate: null, categoryId: null, DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Account.Name.Empty");
    }

    [Fact]
    public void Rename_ShouldFail_WhenNameIsBlank()
    {
        var account = Account.Create(Guid.NewGuid(), "Checking", AccountType.Wallet, dueDate: null, categoryId: null, DateTime.UtcNow).Value;

        var result = account.Rename("   ");

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Rename_ShouldSucceed_AndTrimName()
    {
        var account = Account.Create(Guid.NewGuid(), "Checking", AccountType.Wallet, dueDate: null, categoryId: null, DateTime.UtcNow).Value;

        var result = account.Rename("  Savings  ");

        result.IsSuccess.ShouldBeTrue();
        account.Name.ShouldBe("Savings");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var account = Account.Create(Guid.NewGuid(), "Checking", AccountType.Wallet, dueDate: null, categoryId: null, DateTime.UtcNow).Value;

        var result = account.Deactivate();

        result.IsSuccess.ShouldBeTrue();
        account.IsActive.ShouldBeFalse();
    }
}

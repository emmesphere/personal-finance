using PersonalFinance.Domain.Finance.Categories;
using PersonalFinance.Domain.Finance.Common;

using Shouldly;

namespace PersonalFinance.Domain.Tests.Finance.Categories;

public sealed class CategoryTests
{
    [Fact]
    public void Create_ShouldSucceed_ForUserCreatedCategory()
    {
        var userId = UserId.From(Guid.NewGuid());

        var result = Category.Create("Food", CategoryKind.Expense, userId, isSystemDefined: false, DateTime.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsSystemDefined.ShouldBeFalse();
    }

    [Fact]
    public void Create_ShouldSucceed_ForSystemDefinedCategory_WithoutCreator()
    {
        var result = Category.Create("Food", CategoryKind.Expense, createdByUserId: null, isSystemDefined: true, DateTime.UtcNow);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Create_ShouldFail_WhenUserCreatedCategoryHasNoCreator()
    {
        var result = Category.Create("Food", CategoryKind.Expense, createdByUserId: null, isSystemDefined: false, DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.CreatedByUserId.Required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenNameIsBlank(string name)
    {
        var result = Category.Create(name, CategoryKind.Expense, UserId.From(Guid.NewGuid()), isSystemDefined: false, DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.Name.Empty");
    }

    [Fact]
    public void EnsureEditableBy_ShouldSucceed_WhenRequesterIsAdmin()
    {
        var category = Category.Create("Food", CategoryKind.Expense, UserId.From(Guid.NewGuid()), isSystemDefined: false, DateTime.UtcNow).Value;

        var result = category.EnsureEditableBy(UserId.From(Guid.NewGuid()), requesterIsAdmin: true);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void EnsureEditableBy_ShouldSucceed_WhenRequesterIsCreator()
    {
        var creator = UserId.From(Guid.NewGuid());
        var category = Category.Create("Food", CategoryKind.Expense, creator, isSystemDefined: false, DateTime.UtcNow).Value;

        var result = category.EnsureEditableBy(creator, requesterIsAdmin: false);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void EnsureEditableBy_ShouldFail_WhenRequesterIsNeitherCreatorNorAdmin()
    {
        var category = Category.Create("Food", CategoryKind.Expense, UserId.From(Guid.NewGuid()), isSystemDefined: false, DateTime.UtcNow).Value;

        var result = category.EnsureEditableBy(UserId.From(Guid.NewGuid()), requesterIsAdmin: false);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.NotEditable");
    }

    [Fact]
    public void EnsureEditableBy_ShouldFail_ForSystemDefinedCategory_WhenRequesterIsNotAdmin()
    {
        var category = Category.Create("Food", CategoryKind.Expense, createdByUserId: null, isSystemDefined: true, DateTime.UtcNow).Value;

        var result = category.EnsureEditableBy(UserId.From(Guid.NewGuid()), requesterIsAdmin: false);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.NotEditable");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var category = Category.Create("Food", CategoryKind.Expense, UserId.From(Guid.NewGuid()), isSystemDefined: false, DateTime.UtcNow).Value;

        category.Deactivate();

        category.IsActive.ShouldBeFalse();
    }
}

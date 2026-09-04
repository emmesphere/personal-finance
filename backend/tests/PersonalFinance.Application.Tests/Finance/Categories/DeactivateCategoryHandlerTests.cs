using PersonalFinance.Application.Finance.Categories.DeactivateCategory;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Finance.Categories;
using PersonalFinance.Domain.Finance.Common;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Categories;

public sealed class DeactivateCategoryHandlerTests
{
    private readonly FakeCategoryRepository _categoryRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly DateTime _now = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    private DeactivateCategoryHandler CreateHandler(Guid userId, bool isAdmin = false)
        => new(_categoryRepository, _unitOfWork, new FakeCurrentUserService(userId, isAdmin), new DeactivateCategoryValidator());

    [Fact]
    public async Task HandleAsync_ShouldDeactivate_WhenRequesterIsCreator()
    {
        var creatorId = Guid.NewGuid();
        var category = Category.Create("Groceries", CategoryKind.Expense, UserId.From(creatorId), isSystemDefined: false, _now).Value;
        _categoryRepository.Seed(category);

        var result = await CreateHandler(creatorId).HandleAsync(new DeactivateCategoryCommand(category.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        category.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldDeactivate_WhenRequesterIsAdmin()
    {
        var category = Category.Create("Groceries", CategoryKind.Expense, createdByUserId: null, isSystemDefined: true, _now).Value;
        _categoryRepository.Seed(category);

        var result = await CreateHandler(Guid.NewGuid(), isAdmin: true).HandleAsync(new DeactivateCategoryCommand(category.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        category.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenRequesterIsNeitherCreatorNorAdmin()
    {
        var category = Category.Create("Groceries", CategoryKind.Expense, UserId.From(Guid.NewGuid()), isSystemDefined: false, _now).Value;
        _categoryRepository.Seed(category);

        var result = await CreateHandler(Guid.NewGuid()).HandleAsync(new DeactivateCategoryCommand(category.Id), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.NotEditable");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenCategoryNotFound()
    {
        var result = await CreateHandler(Guid.NewGuid()).HandleAsync(new DeactivateCategoryCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.NotFound");
    }
}

using PersonalFinance.Application.Finance.Categories.CreateCategory;
using PersonalFinance.Application.Tests.Fakes;
using PersonalFinance.Domain.Finance.Categories;

using Shouldly;

namespace PersonalFinance.Application.Tests.Finance.Categories;

public sealed class CreateCategoryHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly FakeCategoryRepository _categoryRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _dateTimeProvider = new(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));

    private CreateCategoryHandler CreateHandler()
        => new(_categoryRepository, _unitOfWork, _dateTimeProvider, new FakeCurrentUserService(_userId), new CreateCategoryValidator());

    [Fact]
    public async Task HandleAsync_ShouldCreateCategory_OwnedByCurrentUser()
    {
        var command = new CreateCategoryCommand("Groceries", CategoryKind.Expense);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _unitOfWork.SaveChangesCallCount.ShouldBe(1);

        var stored = (await _categoryRepository.ListActiveByKindAsync(null, CancellationToken.None)).Single();
        stored.CreatedByUserId!.Value.ShouldBe(_userId);
        stored.IsSystemDefined.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenNameIsBlank()
    {
        var command = new CreateCategoryCommand("", CategoryKind.Expense);

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Category.Validation");
    }
}

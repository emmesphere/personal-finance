using FluentValidation;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Categories;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Application.Finance.Categories.CreateCategory;

public sealed class CreateCategoryHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IValidator<CreateCategoryCommand> validator)
{
    public async Task<Result<CreateCategoryResponse>> HandleAsync(CreateCategoryCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<CreateCategoryResponse>(
                ResultError.Validation("Category.Validation", validation.Errors[0].ErrorMessage));
        }

        var userIdResult = UserId.Create(currentUserService.UserId);
        if (userIdResult.IsFailure)
            return Result.Failure<CreateCategoryResponse>(userIdResult.Error);

        var categoryResult = Category.Create(
            command.Name,
            command.Kind,
            userIdResult.Value,
            isSystemDefined: false,
            dateTimeProvider.UtcNow);

        if (categoryResult.IsFailure)
            return Result.Failure<CreateCategoryResponse>(categoryResult.Error);

        var category = categoryResult.Value;

        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new CreateCategoryResponse(category.Id));
    }
}

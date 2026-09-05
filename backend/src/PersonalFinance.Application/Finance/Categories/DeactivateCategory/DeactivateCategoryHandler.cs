using FluentValidation;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Application.Finance.Categories.DeactivateCategory;

public sealed class DeactivateCategoryHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IValidator<DeactivateCategoryCommand> validator)
{
    public async Task<Result> HandleAsync(DeactivateCategoryCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return Result.Failure(ResultError.Validation("Category.Validation", validation.Errors[0].ErrorMessage));

        var category = await categoryRepository.GetByIdAsync(command.CategoryId, ct);
        if (category is null)
            return Result.Failure(ResultError.NotFound("Category.NotFound", "Category not found."));

        var userIdResult = UserId.Create(currentUserService.UserId);
        if (userIdResult.IsFailure)
            return Result.Failure(userIdResult.Error);

        var editCheck = category.EnsureEditableBy(userIdResult.Value, currentUserService.IsAdmin);
        if (editCheck.IsFailure)
            return editCheck;

        var deactivateResult = category.Deactivate();
        if (deactivateResult.IsFailure)
            return deactivateResult;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

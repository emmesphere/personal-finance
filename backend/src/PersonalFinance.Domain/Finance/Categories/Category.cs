using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Domain.Finance.Categories;

public sealed class Category : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public CategoryKind Kind { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public bool IsSystemDefined { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Category() { }

    private Category(string name, CategoryKind kind, UserId? createdByUserId, bool isSystemDefined, DateTime createdAt)
    {
        Name = name;
        Kind = kind;
        CreatedByUserId = createdByUserId;
        IsSystemDefined = isSystemDefined;
        CreatedAt = createdAt;
        IsActive = true;
    }

    public static Result<Category> Create(
        string name,
        CategoryKind kind,
        UserId? createdByUserId,
        bool isSystemDefined,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Category>(ResultError.Validation("Category.Name.Empty", "Category name is required."));

        if (!isSystemDefined && createdByUserId is null)
            return Result.Failure<Category>(ResultError.Validation("Category.CreatedByUserId.Required", "A user-created category must have a creator."));

        return Result.Success(new Category(name.Trim(), kind, createdByUserId, isSystemDefined, createdAt));
    }

    public Result EnsureEditableBy(UserId requestingUserId, bool requesterIsAdmin)
    {
        if (requesterIsAdmin)
            return Result.Success();

        if (CreatedByUserId is not null && CreatedByUserId.Equals(requestingUserId))
            return Result.Success();

        return Result.Failure(ResultError.Conflict("Category.NotEditable", "You can only manage categories you created."));
    }

    public Result Deactivate()
    {
        IsActive = false;
        return Result.Success();
    }
}

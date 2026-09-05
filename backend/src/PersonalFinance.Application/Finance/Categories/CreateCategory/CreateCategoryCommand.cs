using PersonalFinance.Domain.Finance.Categories;

namespace PersonalFinance.Application.Finance.Categories.CreateCategory;

public sealed record CreateCategoryCommand(string Name, CategoryKind Kind);

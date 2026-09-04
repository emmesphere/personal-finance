using PersonalFinance.Domain.Finance.Categories;

namespace PersonalFinance.Application.Finance.Categories.ListCategories;

public sealed record CategorySummary(Guid Id, string Name, CategoryKind Kind, bool IsSystemDefined);

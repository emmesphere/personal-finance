using PersonalFinance.Domain.Finance.Categories;

namespace PersonalFinance.WebApi.Contracts;

internal sealed record CreateCategoryRequest(string Name, CategoryKind Kind);

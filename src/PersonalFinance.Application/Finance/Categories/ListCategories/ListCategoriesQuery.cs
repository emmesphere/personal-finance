using PersonalFinance.Domain.Finance.Categories;

namespace PersonalFinance.Application.Finance.Categories.ListCategories;

public sealed record ListCategoriesQuery(CategoryKind? Kind);

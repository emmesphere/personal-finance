using Microsoft.Extensions.DependencyInjection;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.Domain.Finance.Categories;

namespace PersonalFinance.Infrastructure.Seeding;

public static class CategorySeeder
{
    private static readonly string[] DefaultIncomeCategories =
    [
        "Wages",
        "Salary",
        "Meal Voucher",
        "Food Voucher",
        "Transport Voucher",
        "Fuel Voucher",
        "Earnings",
        "Rents",
        "Pensions",
    ];

    private static readonly string[] DefaultExpenseCategories =
    [
        "Food",
        "Transportation",
        "Leisure",
        "Health and Wellness",
        "Education",
        "Housing",
        "Pets",
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        var categoryRepository = services.GetRequiredService<ICategoryRepository>();
        var dateTimeProvider = services.GetRequiredService<IDateTimeProvider>();

        var existing = await categoryRepository.ListActiveByKindAsync(kind: null, ct);
        if (existing.Count > 0)
            return;

        foreach (var name in DefaultIncomeCategories)
            AddDefault(categoryRepository, name, CategoryKind.Income, dateTimeProvider.UtcNow);

        foreach (var name in DefaultExpenseCategories)
            AddDefault(categoryRepository, name, CategoryKind.Expense, dateTimeProvider.UtcNow);

        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static void AddDefault(ICategoryRepository repository, string name, CategoryKind kind, DateTime createdAt)
    {
        var result = Category.Create(name, kind, createdByUserId: null, isSystemDefined: true, createdAt);
        if (result.IsSuccess)
            repository.Add(result.Value);
    }
}

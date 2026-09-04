using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using PersonalFinance.Application.Finance.Accounts.CreateAccount;
using PersonalFinance.Application.Finance.Budgets.SetMonthlyBudget;
using PersonalFinance.Application.Finance.Categories.CreateCategory;
using PersonalFinance.Application.Finance.Categories.DeactivateCategory;
using PersonalFinance.Application.Finance.Common;
using PersonalFinance.Application.Finance.Expenses.AddExpense;
using PersonalFinance.Application.Finance.Identity.DeactivateUser;
using PersonalFinance.Application.Finance.Identity.DemoteUser;
using PersonalFinance.Application.Finance.Identity.LoginUser;
using PersonalFinance.Application.Finance.Identity.PromoteUser;
using PersonalFinance.Application.Finance.Identity.RegisterUser;
using PersonalFinance.Application.Finance.Incomes.AddIncome;
using PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;

namespace PersonalFinance.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Validator
        services.AddScoped<IValidator<PostJournalEntryCommand>, PostJournalEntryValidator>();
        services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserValidator>();
        services.AddScoped<IValidator<LoginUserCommand>, LoginUserValidator>();
        services.AddScoped<IValidator<CreateAccountCommand>, CreateAccountValidator>();
        services.AddScoped<IValidator<CreateCategoryCommand>, CreateCategoryValidator>();
        services.AddScoped<IValidator<DeactivateCategoryCommand>, DeactivateCategoryValidator>();
        services.AddScoped<IValidator<AddIncomeCommand>, AddIncomeValidator>();
        services.AddScoped<IValidator<AddExpenseCommand>, AddExpenseValidator>();
        services.AddScoped<IValidator<SetMonthlyBudgetCommand>, SetMonthlyBudgetValidator>();
        services.AddScoped<IValidator<DeactivateUserCommand>, DeactivateUserValidator>();
        services.AddScoped<IValidator<PromoteUserCommand>, PromoteUserValidator>();
        services.AddScoped<IValidator<DemoteUserCommand>, DemoteUserValidator>();

        services.AddScoped<CategoryBackingAccountProvisioner>();

        return services;
    }
}

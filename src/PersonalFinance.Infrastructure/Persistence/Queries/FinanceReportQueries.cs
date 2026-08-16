using Microsoft.EntityFrameworkCore;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.JournalEntries;

namespace PersonalFinance.Infrastructure.Persistence.Queries;

public sealed class FinanceReportQueries(PersonalFinanceDbContext context) : IFinanceReportQueries
{
    private static readonly AccountType[] AssetLikeTypes =
    [
        AccountType.BankAccount,
        AccountType.Wallet,
        AccountType.Benefit,
        AccountType.Debit,
    ];

    public async Task<decimal> GetTotalBalanceAsync(Guid ledgerId, CancellationToken ct)
    {
        var rows = await (
            from je in context.JournalEntries.AsNoTracking()
            where je.LedgerId == ledgerId && je.Status == JournalEntryStatus.Posted
            from line in je.Lines
            join account in context.Accounts.AsNoTracking() on line.AccountId equals account.Id
            where AssetLikeTypes.Contains(account.Type)
            select new { line.Type, line.Amount }
        ).ToListAsync(ct);

        return rows.Where(r => r.Type == EntryType.Debit).Sum(r => r.Amount.Amount)
             - rows.Where(r => r.Type == EntryType.Credit).Sum(r => r.Amount.Amount);
    }

    public async Task<IReadOnlyCollection<CategoryAmount>> GetExpensesByCategoryAsync(Guid ledgerId, YearMonth yearMonth, CancellationToken ct)
    {
        var rows = await (
            from je in context.JournalEntries.AsNoTracking()
            where je.LedgerId == ledgerId
                  && je.Status == JournalEntryStatus.Posted
                  && je.Date.Year == yearMonth.Year
                  && je.Date.Month == yearMonth.Month
            from line in je.Lines
            join account in context.Accounts.AsNoTracking() on line.AccountId equals account.Id
            where account.Type == AccountType.Expense
            select new { account.CategoryId, line.Type, line.Amount }
        ).ToListAsync(ct);

        if (rows.Count == 0)
            return [];

        var categoryIds = rows.Select(r => r.CategoryId!.Value).Distinct().ToList();
        var categoryNames = await context.Categories.AsNoTracking()
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        return rows
            .GroupBy(r => r.CategoryId!.Value)
            .Select(g => new CategoryAmount(
                g.Key,
                categoryNames.GetValueOrDefault(g.Key, "Unknown"),
                g.Where(x => x.Type == EntryType.Debit).Sum(x => x.Amount.Amount)
                    - g.Where(x => x.Type == EntryType.Credit).Sum(x => x.Amount.Amount)))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<MonthlyAmount>> GetYearlyExpenseTotalsAsync(Guid ledgerId, int year, CancellationToken ct)
    {
        var rows = await (
            from je in context.JournalEntries.AsNoTracking()
            where je.LedgerId == ledgerId && je.Status == JournalEntryStatus.Posted && je.Date.Year == year
            from line in je.Lines
            join account in context.Accounts.AsNoTracking() on line.AccountId equals account.Id
            where account.Type == AccountType.Expense
            select new { je.Date.Month, line.Type, line.Amount }
        ).ToListAsync(ct);

        return rows
            .GroupBy(r => r.Month)
            .Select(g => new MonthlyAmount(
                g.Key,
                g.Where(x => x.Type == EntryType.Debit).Sum(x => x.Amount.Amount)
                    - g.Where(x => x.Type == EntryType.Credit).Sum(x => x.Amount.Amount)))
            .OrderBy(m => m.Month)
            .ToArray();
    }
}

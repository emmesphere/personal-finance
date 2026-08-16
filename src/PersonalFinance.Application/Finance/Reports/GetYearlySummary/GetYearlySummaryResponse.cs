using PersonalFinance.Application.Abstractions.Persistence;

namespace PersonalFinance.Application.Finance.Reports.GetYearlySummary;

public sealed record GetYearlySummaryResponse(int Year, IReadOnlyCollection<MonthlyAmount> Months);

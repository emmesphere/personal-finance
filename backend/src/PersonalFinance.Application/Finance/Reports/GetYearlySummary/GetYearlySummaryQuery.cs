namespace PersonalFinance.Application.Finance.Reports.GetYearlySummary;

public sealed record GetYearlySummaryQuery(Guid LedgerId, int Year);

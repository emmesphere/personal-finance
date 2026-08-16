namespace PersonalFinance.Application.Finance.Reports.GetDashboard;

public sealed record GetDashboardQuery(Guid LedgerId, int Year, int Month);

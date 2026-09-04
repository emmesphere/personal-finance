namespace PersonalFinance.Application.Finance.Reports.GetAdminSummary;

public sealed record GetAdminSummaryResponse(int TotalUsers, int TotalLedgers, int PostedJournalEntriesThisMonth);

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using Shouldly;

namespace PersonalFinance.WebApi.Tests.Reports;

public sealed class DashboardAndYearlySummaryTests : IClassFixture<WebApplicationFactory<WebApiMarker>>
{
    private readonly HttpClient _httpClient;

    public DashboardAndYearlySummaryTests(WebApplicationFactory<WebApiMarker> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Dashboard_AndYearlySummary_ShouldReflectPostedIncomeAndExpenses()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var registerResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Report Tester",
            username = $"report-{suffix}",
            email = $"report-{suffix}@example.com",
            phoneNumber = (string?)null,
            password = "supersecret1",
        });
        registerResponse.EnsureSuccessStatusCode();
        using var registerBody = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync());
        var ledgerId = registerBody.RootElement.GetProperty("ledgerId").GetString();

        var loginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new { username = $"report-{suffix}", password = "supersecret1" });
        loginResponse.EnsureSuccessStatusCode();
        using var loginBody = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginBody.RootElement.GetProperty("accessToken").GetString();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var bankId = await CreateAccountAsync(ledgerId!, "Bank", "BankAccount", null);
        var cardId = await CreateAccountAsync(ledgerId!, "Card", "CreditCard", 10);

        var salaryCategoryId = await GetCategoryIdAsync("Income", "Salary");
        var foodCategoryId = await GetCategoryIdAsync("Expense", "Food");

        // Income: +5000 to Bank in January.
        var incomeResponse = await _httpClient.PostAsJsonAsync($"/api/ledgers/{ledgerId}/incomes", new
        {
            categoryId = salaryCategoryId,
            receivingAccountId = bankId,
            amount = 5000m,
            date = "2026-01-15",
            description = "January salary",
        });
        incomeResponse.EnsureSuccessStatusCode();

        // Expense: -200 from Bank in January (reduces balance, hits January expenses).
        var bankExpenseResponse = await _httpClient.PostAsJsonAsync($"/api/ledgers/{ledgerId}/expenses", new
        {
            categoryId = foodCategoryId,
            paymentAccountId = bankId,
            amount = 200m,
            date = "2026-01-10",
            description = "Groceries",
            installmentCount = (int?)null,
        });
        bankExpenseResponse.EnsureSuccessStatusCode();

        // Expense: 300 on Card in 3 installments starting January (does not touch balance, spreads Jan/Feb/Mar).
        var cardExpenseResponse = await _httpClient.PostAsJsonAsync($"/api/ledgers/{ledgerId}/expenses", new
        {
            categoryId = foodCategoryId,
            paymentAccountId = cardId,
            amount = 300m,
            date = "2026-01-20",
            description = "Big shop",
            installmentCount = 3,
        });
        cardExpenseResponse.EnsureSuccessStatusCode();

        // Budget: 1000 for January.
        var budgetResponse = await _httpClient.PutAsJsonAsync($"/api/ledgers/{ledgerId}/budgets/2026/1", new { amount = 1000m });
        budgetResponse.EnsureSuccessStatusCode();

        var dashboardResponse = await _httpClient.GetAsync(new Uri($"/api/ledgers/{ledgerId}/reports/dashboard?year=2026&month=1", UriKind.Relative));
        dashboardResponse.EnsureSuccessStatusCode();
        using var dashboard = JsonDocument.Parse(await dashboardResponse.Content.ReadAsStringAsync());

        dashboard.RootElement.GetProperty("totalBalance").GetDecimal().ShouldBe(4800m);
        dashboard.RootElement.GetProperty("totalExpenses").GetDecimal().ShouldBe(300m);
        dashboard.RootElement.GetProperty("budgetAmount").GetDecimal().ShouldBe(1000m);

        var categoryBreakdown = dashboard.RootElement.GetProperty("expensesByCategory");
        categoryBreakdown.GetArrayLength().ShouldBe(1);
        categoryBreakdown[0].GetProperty("categoryName").GetString().ShouldBe("Food");
        categoryBreakdown[0].GetProperty("amount").GetDecimal().ShouldBe(300m);

        var yearlyResponse = await _httpClient.GetAsync(new Uri($"/api/ledgers/{ledgerId}/reports/yearly-summary?year=2026", UriKind.Relative));
        yearlyResponse.EnsureSuccessStatusCode();
        using var yearly = JsonDocument.Parse(await yearlyResponse.Content.ReadAsStringAsync());

        var months = yearly.RootElement.GetProperty("months").EnumerateArray()
            .ToDictionary(m => m.GetProperty("month").GetInt32(), m => m.GetProperty("amount").GetDecimal());

        months[1].ShouldBe(300m);
        months[2].ShouldBe(100m);
        months[3].ShouldBe(100m);
    }

    private async Task<string> CreateAccountAsync(string ledgerId, string name, string type, int? dueDateDay)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/ledgers/{ledgerId}/accounts", new { name, type, dueDateDay });
        response.EnsureSuccessStatusCode();
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("accountId").GetString()!;
    }

    private async Task<string> GetCategoryIdAsync(string kind, string name)
    {
        var response = await _httpClient.GetAsync(new Uri($"/api/categories?kind={kind}", UriKind.Relative));
        response.EnsureSuccessStatusCode();
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.EnumerateArray()
            .Single(c => c.GetProperty("name").GetString() == name)
            .GetProperty("id").GetString()!;
    }
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using Shouldly;

namespace PersonalFinance.WebApi.Tests.Accounts;

public sealed class AccountBalanceTests : IClassFixture<WebApplicationFactory<WebApiMarker>>
{
    private readonly HttpClient _httpClient;

    public AccountBalanceTests(WebApplicationFactory<WebApiMarker> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ListAccounts_ShouldReportAvailableBalanceForAssets_AndOwedBalanceForCreditCards()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var registerResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Balance Tester",
            username = $"balance-{suffix}",
            email = $"balance-{suffix}@example.com",
            phoneNumber = (string?)null,
            password = "supersecret1",
        });
        registerResponse.EnsureSuccessStatusCode();
        using var registerBody = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync());
        var ledgerId = registerBody.RootElement.GetProperty("ledgerId").GetString();

        var loginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new { username = $"balance-{suffix}", password = "supersecret1" });
        loginResponse.EnsureSuccessStatusCode();
        using var loginBody = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginBody.RootElement.GetProperty("accessToken").GetString();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var bankId = await CreateAccountAsync(ledgerId!, "Bank", "BankAccount", null);
        var cardId = await CreateAccountAsync(ledgerId!, "Card", "CreditCard", 10);

        var salaryCategoryId = await GetCategoryIdAsync("Income", "Salary");
        var foodCategoryId = await GetCategoryIdAsync("Expense", "Food");

        var incomeResponse = await _httpClient.PostAsJsonAsync($"/api/ledgers/{ledgerId}/incomes", new
        {
            categoryId = salaryCategoryId,
            receivingAccountId = bankId,
            amount = 1000m,
            date = "2026-01-15",
            description = "Salary",
        });
        incomeResponse.EnsureSuccessStatusCode();

        var cardExpenseResponse = await _httpClient.PostAsJsonAsync($"/api/ledgers/{ledgerId}/expenses", new
        {
            categoryId = foodCategoryId,
            paymentAccountId = cardId,
            amount = 150m,
            date = "2026-01-20",
            description = "Groceries on card",
            installmentCount = (int?)null,
        });
        cardExpenseResponse.EnsureSuccessStatusCode();

        var accountsResponse = await _httpClient.GetAsync(new Uri($"/api/ledgers/{ledgerId}/accounts", UriKind.Relative));
        accountsResponse.EnsureSuccessStatusCode();
        using var accounts = JsonDocument.Parse(await accountsResponse.Content.ReadAsStringAsync());

        var bank = accounts.RootElement.EnumerateArray().Single(a => a.GetProperty("id").GetString() == bankId);
        var card = accounts.RootElement.EnumerateArray().Single(a => a.GetProperty("id").GetString() == cardId);

        bank.GetProperty("balance").GetDecimal().ShouldBe(1000m);
        card.GetProperty("balance").GetDecimal().ShouldBe(150m);
    }

    [Fact]
    public async Task CreateAccount_ShouldApplyOpeningBalance_AsAvailableForAssets_AndOwedForLiabilities()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var registerResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Opening Balance Tester",
            username = $"opening-{suffix}",
            email = $"opening-{suffix}@example.com",
            phoneNumber = (string?)null,
            password = "supersecret1",
        });
        registerResponse.EnsureSuccessStatusCode();
        using var registerBody = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync());
        var ledgerId = registerBody.RootElement.GetProperty("ledgerId").GetString();

        var loginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new { username = $"opening-{suffix}", password = "supersecret1" });
        loginResponse.EnsureSuccessStatusCode();
        using var loginBody = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginBody.RootElement.GetProperty("accessToken").GetString();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var walletId = await CreateAccountAsync(ledgerId!, "Wallet", "Wallet", null, openingBalance: 2000m);
        var cardId = await CreateAccountAsync(ledgerId!, "Card", "CreditCard", 10, openingBalance: 500m);
        var loanId = await CreateAccountAsync(ledgerId!, "Car Loan", "Loan", 5, openingBalance: 500m);

        var accountsResponse = await _httpClient.GetAsync(new Uri($"/api/ledgers/{ledgerId}/accounts", UriKind.Relative));
        accountsResponse.EnsureSuccessStatusCode();
        using var accounts = JsonDocument.Parse(await accountsResponse.Content.ReadAsStringAsync());

        var wallet = accounts.RootElement.EnumerateArray().Single(a => a.GetProperty("id").GetString() == walletId);
        var card = accounts.RootElement.EnumerateArray().Single(a => a.GetProperty("id").GetString() == cardId);
        var loan = accounts.RootElement.EnumerateArray().Single(a => a.GetProperty("id").GetString() == loanId);

        wallet.GetProperty("balance").GetDecimal().ShouldBe(2000m);
        card.GetProperty("balance").GetDecimal().ShouldBe(500m);
        loan.GetProperty("balance").GetDecimal().ShouldBe(500m);

        // The system-managed "Opening Balance Equity" account must never be exposed in the list.
        accounts.RootElement.EnumerateArray().ShouldAllBe(a => a.GetProperty("type").GetString() != "Equity");
    }

    private async Task<string> CreateAccountAsync(string ledgerId, string name, string type, int? dueDateDay, decimal? openingBalance = null)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/ledgers/{ledgerId}/accounts", new { name, type, dueDateDay, openingBalance });
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

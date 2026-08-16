using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using PersonalFinance.Application;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.Domain.Identity.Users;
using PersonalFinance.Infrastructure;
using PersonalFinance.Infrastructure.Persistence;
using PersonalFinance.Infrastructure.Security;
using PersonalFinance.Infrastructure.Seeding;
using PersonalFinance.WebApi.Endpoints.Accounts;
using PersonalFinance.WebApi.Endpoints.Auth;
using PersonalFinance.WebApi.Endpoints.Admin;
using PersonalFinance.WebApi.Endpoints.Budgets;
using PersonalFinance.WebApi.Endpoints.Categories;
using PersonalFinance.WebApi.Endpoints.Expenses;
using PersonalFinance.WebApi.Endpoints.Incomes;
using PersonalFinance.WebApi.Endpoints.JournalEntries;
using PersonalFinance.WebApi.Endpoints.Reports;
using PersonalFinance.WebApi.Security;

using Serilog;

using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(PersonalFinanceApplicationMarker).Assembly);

    // PersonalFinanceDbContext is registered via AddDbContext's factory lambda, which Wolverine's
    // handler codegen cannot inline-construct. Without this, every handler whose dependency chain
    // reaches the DbContext throws InvalidServiceLocationException at runtime (Wolverine 6+ default).
    opts.CodeGeneration.AlwaysUseServiceLocationFor<PersonalFinanceDbContext>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SigningKey"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PersonalFinanceDbContext>();
    await dbContext.Database.MigrateAsync();

    await SeedAdminUserAsync(scope.ServiceProvider, app.Configuration);
    await CategorySeeder.SeedAsync(scope.ServiceProvider, CancellationToken.None);
}

app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.Use((context, next) =>
{
    var headers = context.Response.Headers;

    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=(), usb=()";

    return next(context);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { status = "ok" }));
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.MapRegisterUser();
app.MapLoginUser();

var protectedApi = app.MapGroup(string.Empty).RequireAuthorization();
protectedApi.MapPostJournalEntry();
protectedApi.MapCreateAccount();
protectedApi.MapListAccounts();
protectedApi.MapCreateCategory();
protectedApi.MapDeactivateCategory();
protectedApi.MapListCategories();
protectedApi.MapAddIncome();
protectedApi.MapAddExpense();
protectedApi.MapSetMonthlyBudget();
protectedApi.MapGetDashboard();
protectedApi.MapGetYearlySummary();

app.MapListUsers();
app.MapDeactivateUser();
app.MapPromoteUser();
app.MapDemoteUser();
app.MapGetAdminSummary();

try
{
    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}

static async Task SeedAdminUserAsync(IServiceProvider services, IConfiguration configuration)
{
    var username = configuration["Admin:Username"];
    var password = configuration["Admin:Password"];

    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        return;

    var userRepository = services.GetRequiredService<IUserRepository>();
    if (await userRepository.ExistsByUsernameAsync(username, CancellationToken.None))
        return;

    var passwordHasher = services.GetRequiredService<IPasswordHasher>();
    var dateTimeProvider = services.GetRequiredService<IDateTimeProvider>();

    var adminResult = User.Register(
        "Administrator",
        username,
        configuration["Admin:Email"],
        null,
        passwordHasher.Hash(password),
        dateTimeProvider.UtcNow);

    if (adminResult.IsFailure)
        return;

    var admin = adminResult.Value;
    admin.PromoteToAdmin();

    userRepository.Add(admin);

    var unitOfWork = services.GetRequiredService<IUnitOfWork>();
    await unitOfWork.SaveChangesAsync(CancellationToken.None);
}

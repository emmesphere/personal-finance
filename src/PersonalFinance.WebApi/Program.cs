using PersonalFinance.Application;
using PersonalFinance.Infrastructure;
using PersonalFinance.WebApi.Endpoints.ToDos;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

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

app.MapGet("/", () => Results.Ok(new { status = "ok" }));
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.MapCreateToDo();

try
{
    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}


# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Packages.props ./
COPY Directory.Build.props ./

COPY src/PersonalFinance.WebApi/PersonalFinance.WebApi.csproj src/PersonalFinance.WebApi/
COPY src/PersonalFinance.Application/PersonalFinance.Application.csproj src/PersonalFinance.Application/
COPY src/PersonalFinance.Infrastructure/PersonalFinance.Infrastructure.csproj src/PersonalFinance.Infrastructure/
COPY src/PersonalFinance.Domain/PersonalFinance.Domain.csproj src/PersonalFinance.Domain/
COPY src/PersonalFinance.BuildingBlocks/PersonalFinance.BuildingBlocks.csproj src/PersonalFinance.BuildingBlocks/

RUN dotnet restore src/PersonalFinance.WebApi/PersonalFinance.WebApi.csproj

COPY . ./

RUN dotnet publish src/PersonalFinance.WebApi/PersonalFinance.WebApi.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "PersonalFinance.WebApi.dll"]

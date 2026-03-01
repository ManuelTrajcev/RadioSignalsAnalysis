# ── Build stage ──
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY RadioSignals.sln ./
COPY Domain/Domain.csproj              Domain/
COPY Repository/Repository.csproj      Repository/
COPY Services/Services.csproj          Services/
COPY RadioSignalsWeb/RadioSignalsWeb.csproj RadioSignalsWeb/

# Restore
RUN dotnet restore RadioSignals.sln

# Copy everything else
COPY . .

RUN dotnet publish RadioSignalsWeb/RadioSignalsWeb.csproj \
    -c Release \
    -o /app/publish

# ── Runtime stage ──
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Copy seed data next to the published binary
COPY RadioSignalsWeb/SeedData ./SeedData

# Default port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RadioSignalsWeb.dll"]


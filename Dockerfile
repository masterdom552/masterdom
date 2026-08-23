# Multi-stage Dockerfile for Masterdom.Host
# Deployment baseline: ef09797 (committed)
# This image builds only from committed ef09797, excluding all pre-existing working-tree changes

# Stage 1: Builder
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /build

# Copy solution and package management files
COPY ./Masterdom.slnx .
COPY ./Directory.Build.props .
COPY ./Directory.Packages.props .
COPY ./global.json .

# Copy core projects
COPY ./src/Masterdom.Abstractions/ ./src/Masterdom.Abstractions/
COPY ./src/Masterdom.Core/ ./src/Masterdom.Core/
COPY ./src/Masterdom.Platform/ ./src/Masterdom.Platform/
COPY ./src/Masterdom.Infrastructure/ ./src/Masterdom.Infrastructure/
COPY ./src/Masterdom.Host/ ./src/Masterdom.Host/

# Copy business modules (committed state only)
COPY ./src/Masterdom.Modules.Security/ ./src/Masterdom.Modules.Security/
COPY ./src/Masterdom.Modules.UtilityRating/ ./src/Masterdom.Modules.UtilityRating/
COPY ./src/Masterdom.Modules.CRM/ ./src/Masterdom.Modules.CRM/
COPY ./src/Masterdom.Modules.Properties/ ./src/Masterdom.Modules.Properties/
COPY ./src/Masterdom.Modules.People/ ./src/Masterdom.Modules.People/
COPY ./src/Masterdom.Modules.Lease/ ./src/Masterdom.Modules.Lease/
COPY ./src/Masterdom.Modules.Tenancy/ ./src/Masterdom.Modules.Tenancy/
COPY ./src/Masterdom.Modules.Metering/ ./src/Masterdom.Modules.Metering/
COPY ./src/Masterdom.Modules.Maintenance/ ./src/Masterdom.Modules.Maintenance/
COPY ./src/Masterdom.Modules.Inventory/ ./src/Masterdom.Modules.Inventory/
COPY ./src/Masterdom.Modules.Reporting/ ./src/Masterdom.Modules.Reporting/
COPY ./src/Masterdom.Modules.Notifications/ ./src/Masterdom.Modules.Notifications/
COPY ./src/Masterdom.Modules.Documents/ ./src/Masterdom.Modules.Documents/
COPY ./src/Masterdom.Modules.FinancialLedger/ ./src/Masterdom.Modules.FinancialLedger/
COPY ./src/Masterdom.Modules.Billing/ ./src/Masterdom.Modules.Billing/
COPY ./src/Masterdom.Modules.Payment/ ./src/Masterdom.Modules.Payment/
COPY ./src/Masterdom.Modules.SubsidyOptimization/ ./src/Masterdom.Modules.SubsidyOptimization/
COPY ./src/Masterdom.Modules.PolicyFramework/ ./src/Masterdom.Modules.PolicyFramework/
COPY ./src/Masterdom.Modules.Settings/ ./src/Masterdom.Modules.Settings/
COPY ./src/Masterdom.Modules.Intelligence/ ./src/Masterdom.Modules.Intelligence/
COPY ./src/Masterdom.Modules.Finance/ ./src/Masterdom.Modules.Finance/

# Copy test projects
COPY ./tests/ ./tests/

# Restore (solution to get all dependencies)
RUN dotnet restore Masterdom.slnx

# Build Host project (which transitively builds all production dependencies)
RUN dotnet build src/Masterdom.Host/Masterdom.Host.csproj -c Release --no-restore

# Publish
RUN dotnet publish src/Masterdom.Host/Masterdom.Host.csproj -c Release -o /app/publish --no-build

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copy published application
COPY --from=builder /app/publish .

# Expose port
EXPOSE 5000

# Entry point
ENTRYPOINT ["dotnet", "Masterdom.Host.dll"]

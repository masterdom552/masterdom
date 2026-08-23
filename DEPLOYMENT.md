# Masterdom Deployment Guide

## Deployment Baseline

- **Commit:** ef09797
- **Branch:** main
- **Date:** 2026-08-23
- **Capabilities:** 18 business modules (Property, PolicyFramework, CRM, People, Lease, Tenancy, Metering, Maintenance, Inventory, Billing, UtilityRating, FinancialLedger, Payment, SubsidyOptimization, IdentityAdministration, Reporting, Notifications, Documents)

**Note:** This deployment uses only committed baseline. Pre-existing working-tree changes (Intelligence production implementation, Delegation implementation) are NOT included.

## Architecture

- **Application:** Masterdom.Host (.NET 10 ASP.NET Core)
- **Runtime:** Port 5000 (HTTP)
- **Database:** PostgreSQL 16
- **Storage:** Named volume (postgres_data)
- **Network:** Bridge (masterdom-net)

## Quick Start

### Prerequisites

- Docker and Docker Compose installed
- Network access to Docker Hub (for base images)

### Local Development Deployment

```bash
# Build and start services
docker-compose up --build

# In a new terminal, verify connectivity
curl http://localhost:5000

# Stop services
docker-compose down

# Stop and remove database
docker-compose down -v
```

## Configuration

### Environment Variables

Required at runtime:

- **Authentication__Bearer__SigningKey** (required)
  - Used for JWT bearer token signing
  - Must be at least 32 characters
  - Default in compose: "MasterdomTestKeyFor32CharMinLength!" (development only)

- **MASTERDOM_CONNECTION_STRING** (required)
  - PostgreSQL connection string
  - Default in compose: "Host=postgres;Port=5432;Database=masterdom;User Id=postgres;Password=postgres"

### Custom Configuration

To use custom environment values:

```bash
export AUTH_SIGNING_KEY="your-production-key-here-minimum-32-chars"
docker-compose up --build
```

Or create a `.env` file:

```
AUTH_SIGNING_KEY=your-production-key-here-minimum-32-chars
```

Then run:

```bash
docker-compose up --build
```

## Database

### Initialization

PostgreSQL initializes on first run with:
- Database: masterdom
- User: postgres
- Password: postgres

### Migrations

EF Core migrations are present in the committed ef09797 baseline but are NOT automatically applied by application startup.

**Important:** Database schema must be initialized BEFORE the application starts. The application will fail if the database schema is missing.

**Manual Migration Procedure:**

To initialize the database schema, run the EF Core migrations from the deployment source:

```bash
# Inside the deployment source directory, after building the application:
dotnet ef database update --project src/Masterdom.Infrastructure --startup-project src/Masterdom.Host

# Alternatively, in a Docker environment, a separate init container or script must run migrations before the application container starts.
```

The current Compose configuration does NOT automatically run migrations. This is intentional — schema initialization should be a deliberate, auditable step separate from application startup.

### Persistent Storage

Database data persists in the `postgres_data` Docker volume:

```bash
# List volumes
docker volume ls | grep postgres_data

# Remove volume (destroys data)
docker volume rm masterdom-postgres-postgres_data
```

## Verification

### Application Startup

When services start successfully, you should see:

```
masterdom-host    | info: Microsoft.Hosting.Lifetime[14]
masterdom-host    |       Now listening on: http://localhost:5000
masterdom-host    | info: Microsoft.Hosting.Lifetime[0]
masterdom-host    |       Application started.
```

### PostgreSQL Connectivity

```bash
docker exec masterdom-postgres pg_isready -U postgres -d masterdom
# Should return: "accepting connections"
```

### Application Health

```bash
curl http://localhost:5000/
# Expected: No specific health endpoint in current baseline
# But server should respond (may return 401 if auth required)
```

## Known Limitations

1. **No Health Check Endpoint:** The application does not expose a /health endpoint. Readiness verification relies on successful startup logs.

2. **No Graceful Shutdown Signal Handler:** The application does not implement custom graceful shutdown hooks beyond default ASP.NET Core behavior.

3. **Sticky Session:** Database connection pool does not have advanced tuning; use defaults for this deployment.

4. **Credentials in Compose:** Development compose file uses hardcoded credentials. Replace for production deployments.

## Troubleshooting

### PostgreSQL Fails to Start

```
Error: binding to port 5432 failed
```

Solution: Another service is using port 5432. Change port mapping:
```yaml
postgres:
  ports:
    - "5433:5432"  # Change to unused port
```

### Application Cannot Connect to Database

```
SqlException: A network-related or instance-specific error occurred
```

Solution: Ensure PostgreSQL is healthy:
```bash
docker-compose logs postgres
# Verify "accepting connections" message
```

### Docker Image Build Fails

Ensure Docker has internet access to pull base images:
```bash
docker pull mcr.microsoft.com/dotnet/sdk:10.0
docker pull mcr.microsoft.com/dotnet/aspnet:10.0
```

## Production Considerations

This deployment infrastructure is designed for local development and testing. For production use:

1. **Secrets:** Use Docker secrets or external secret management (Vault, AWS Secrets Manager, etc.)
2. **Database:** Use managed PostgreSQL service (AWS RDS, Azure Database, etc.)
3. **Reverse Proxy:** Add nginx, Traefik, or cloud load balancer for HTTPS
4. **Monitoring:** Integrate with logging and monitoring systems
5. **Resource Limits:** Set CPU and memory limits in compose or orchestrator
6. **Authentication:** Configure OAuth2, OIDC, or enterprise auth
7. **Networking:** Use private networks and security groups

## Support

For issues with the application itself, refer to the main Masterdom repository documentation.

For container/deployment issues, verify:
1. Docker version (must support compose v3.8)
2. Network connectivity
3. Available disk space for database volume
4. Port availability (5000 for app, 5432 for postgres)

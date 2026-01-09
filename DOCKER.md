# Neha Surgical API - Docker Setup

## Quick Start

### Using Docker Compose (Recommended)

1. **Build and start all services:**
   ```bash
   docker-compose up -d
   ```

2. **View logs:**
   ```bash
   docker-compose logs -f neha-surgical-api
   ```

3. **Stop services:**
   ```bash
   docker-compose down
   ```

### Using Docker Only

1. **Build the image:**
   ```bash
   docker build -t neha-surgical-api:latest .
   ```

2. **Run the container:**
   ```bash
   docker run -d \
     -p 5280:8080 \
     -e ConnectionStrings__DefaultConnection="Host=your-db-host;Database=neha_surgical;Username=postgres;Password=your_password" \
     --name neha-surgical-api \
     neha-surgical-api:latest
   ```

## Configuration

### Environment Variables

The application can be configured using environment variables:

- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)
- `ASPNETCORE_URLS` - URLs the app listens on
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string
- `Jwt__Key` - JWT secret key
- `Jwt__Issuer` - JWT issuer
- `Jwt__Audience` - JWT audience
- `Jwt__ExpiryHours` - JWT token expiry in hours

### Ports

- **5280** - API HTTP port
- **5281** - API HTTPS port (optional)
- **5432** - PostgreSQL port

## Database Setup

The PostgreSQL container automatically runs SQL scripts from the `Database/` folder on first startup. Ensure all required schema scripts are in that folder.

## Health Check

The API includes a health check endpoint at `/api/health`. The Docker container automatically monitors this endpoint.

## Production Deployment

1. **Create environment file:**
   ```bash
   cp .env.example .env
   # Edit .env with your production values
   ```

2. **Update docker-compose.yml** with your production settings

3. **Deploy:**
   ```bash
   docker-compose up -d
   ```

## Troubleshooting

**Container won't start:**
```bash
docker logs neha-surgical-api
```

**Database connection issues:**
```bash
docker-compose exec postgres-db psql -U postgres -d neha_surgical
```

**Rebuild after code changes:**
```bash
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## Backup Database

```bash
docker-compose exec postgres-db pg_dump -U postgres neha_surgical > backup.sql
```

## Restore Database

```bash
cat backup.sql | docker-compose exec -T postgres-db psql -U postgres neha_surgical
```

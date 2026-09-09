# Example REST API using C#

This is a small ASP.NET Core example of a server-monitoring API. It demonstrates a lightweight monitoring service that records performance data and alerts when a server is looking unhealthy.

## Detail

It stores telemetry data about a machine, such as CPU and memory usage, and exposes simple REST endpoints to create and query that data (Program.cs).

Data is stored in an SQLite database (ServerMonitorDBContext.cs) which includes:

- server metrics
- application log entries

The app also includes a background worker (TelemetryAlertWorker.cs) that periodically samples the local machine’s CPU usage and writes a warning log if it rises above a threshold (default 80%). In other words, it demonstrates a lightweight monitoring service that records performance data and alerts when a server looks unhealthy.

# Endpoints

The endpoints validate incoming data using the FluentValidation library (see Filters\ValidationFilter.cs)

## POST /api/metrics

Creates a new server metric record

```json
{
  "serverName": "prod-web-01",
  "cpuUsagePercent": 94.2,
  "memoryUsageMb": 8192.0,
  "timestamp": "2026-08-22T22:15:00Z"
}
```

- `serverName` is required
- `cpuUsagePercent` must be between 0 and 100
- `memoryUsageMb` must be 0 or greater
- `timestamp` is optional; if omitted, the server uses current UTC time

### Example

```bash
curl -X POST http://localhost:5000/api/metrics \
  -H "Content-Type: application/json" \
  -d '{
    "serverName": "prod-web-01",
    "cpuUsagePercent": 94.2,
    "memoryUsageMb": 8192.0,
    "timestamp": "2026-08-22T22:15:00Z"
  }'
```

## GET /api/metrics/{servername}

Gets the most recent metrics for a given server.

### Example

```bash
curl http://localhost:5000/api/metrics/prod-web-01
```

Returns:

- up to 50 records
- sorted by newest timestamp first

## POST /api/logs

Adds a log entry

```json
{
  "applicationName": "ServerMonitor",
  "logLevel": "Warning",
  "message": "CPU usage reached 85.0%, above the 80.0% threshold.",
  "timestamp": "2026-08-22T22:15:00Z"
}
```

### Example

```bash
curl -X POST http://localhost:5000/api/logs \
  -H "Content-Type: application/json" \
  -d '{
    "applicationName": "ServerMonitor",
    "logLevel": "Warning",
    "message": "CPU usage reached 85.0%, above the 80.0% threshold.",
    "timestamp": "2026-08-22T22:15:00Z"
  }'
```

Valid log levels are:

- `Info`
- `Warning`
- `Error`

## GET /api/logs

Reads logs, optionally filtered by level.

### Examples

```bash
curl "http://localhost:5000/api/logs?level=Error"
curl "http://localhost:5000/api/logs?level=Warning"
curl http://localhost:5000/api/logs
```

This returns logs ordered newest-first; if `level` is provided, only matching records are returned.

# Default run

```
cd src/ServerMonitor
dotnet run
```

# Running the server (Dockerfile)

## Build

`docker build -t server-monitor .`

## Run

NOTE: If running on Windows, replace `$(pwd)` with: - `$PWD` or `${PWD}` for Powershell - `%cd%` for standard command prompt (and `^` instead of `\`` or `\`)

NOTE 2: The db doesn't persist once the container stops (setup a data folder if needed)

NOTE 3: Set the hostname, so you can query the metrics, else you have to use the docker container ID

NOTE 4: `-e` is used to overide the `appsettings.json` and makes sure the db is mounted inside `/app_data` (alternatively could update `appsettings.json` to point to `/app/data/server-monitor.db`)

```bash
docker run -d \
  --name server-monitor-app \
  --hostname "docker-telemetry-node" \
  -p 5000:8080 \
  -e "ConnectionStrings__ServerMonitor=Data Source=/app/data/server-monitor.db" \
  -v "$(PWD)/app_data:/app/data" \
  server-monitor
```

## Stop and remove

```bash
docker stop server-monitor-app
docker rm server-monitor-app
```

# NOTE For Windows (using curl):

```cmd
curl.exe -X POST http://localhost:5000/api/metrics `  -H "Content-Type: application/json"`
-d '{\"serverName\":\"prod-web-01\",\"cpuUsagePercent\":94.2,\"memoryUsageMb\":8192.0,\"timestamp\":\"2026-08-22T22:15:00Z\"}'
```

# Tests

`dotnet test`

```cmd
cd d:\Development\_Learning\server-monitor
dotnet test .\ServerMonitor.Tests\ServerMonitor.Tests.csproj --nologo
```

```cmd
// Test a class or filter
dotnet test .\ServerMonitor.Tests\ServerMonitor.Tests.csproj --filter CreateMetricRequestValidatorTests
```

# Run the client

`dotnet run --project src/ServerMonitor.Client`

# Test Server API Key

```bash
# Without Valid API Key
curl -i -X POST http://localhost:5000/api/metrics \
  -H "Content-Type: application/json" \
  -d '{"serverName":"prod-web-01","cpuUsagePercent":45.0,"memoryUsageMb":4096.0}'
```

```bash
# With Valid API Key
curl -i -X POST http://localhost:5000/api/metrics \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: Telemetry-Secret-Key-98765" \
  -d '{"serverName":"prod-web-01","cpuUsagePercent":45.0,"memoryUsageMb":4096.0}'
```

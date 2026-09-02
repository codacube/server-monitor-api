# ==========================================
# STAGE 1: Build (.NET 10 SDK)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project file
COPY src/ServerMonitor/ServerMonitor.csproj src/ServerMonitor/

# Restore directly targeting the copied csproj
RUN dotnet restore src/ServerMonitor/ServerMonitor.csproj

# Copy remaining source files and publish
COPY . .
RUN dotnet publish src/ServerMonitor/ServerMonitor.csproj -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# STAGE 2: Runtime (.NET 10 Runtime)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 8080
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "ServerMonitor.dll"]
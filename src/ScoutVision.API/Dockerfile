# Multi-stage build for ScoutVision API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY ["src/ScoutVision.API/ScoutVision.API.csproj", "src/ScoutVision.API/"]
COPY ["src/ScoutVision.Core/ScoutVision.Core.csproj", "src/ScoutVision.Core/"]
COPY ["src/ScoutVision.Infrastructure/ScoutVision.Infrastructure.csproj", "src/ScoutVision.Infrastructure/"]

RUN dotnet restore "src/ScoutVision.API/ScoutVision.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/ScoutVision.API"
RUN dotnet build "ScoutVision.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ScoutVision.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --home /app --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "ScoutVision.API.dll"]
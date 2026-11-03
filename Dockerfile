# syntax=docker/dockerfile:1

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (cached layer) using only project + build-config files.
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/ShipmentTracker.Domain/*.csproj src/ShipmentTracker.Domain/
COPY src/ShipmentTracker.Application/*.csproj src/ShipmentTracker.Application/
COPY src/ShipmentTracker.Infrastructure/*.csproj src/ShipmentTracker.Infrastructure/
COPY src/ShipmentTracker.Api/*.csproj src/ShipmentTracker.Api/
RUN dotnet restore src/ShipmentTracker.Api/ShipmentTracker.Api.csproj

COPY src/ src/
RUN dotnet publish src/ShipmentTracker.Api/ShipmentTracker.Api.csproj \
    --configuration Release --no-restore --output /app/publish

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
# Non-root user that ships with the official .NET images.
USER $APP_UID
ENTRYPOINT ["dotnet", "ShipmentTracker.Api.dll"]

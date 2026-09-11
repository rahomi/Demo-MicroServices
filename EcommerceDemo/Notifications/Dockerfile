# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy Contracts project (shared dependency)
COPY Contracts/Contracts.csproj Contracts/
# Copy service project
COPY Notifications/Notifications.csproj Notifications/

RUN dotnet restore Notifications/Notifications.csproj

# Copy all source and build
COPY Contracts/ Contracts/
COPY Notifications/ Notifications/

WORKDIR /src/Notifications
RUN dotnet publish -c Release -o /app/publish --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Notifications.dll"]

# --- Base Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiere Projektdatei und restore NuGet-Pakete
COPY ./backend.csproj ./Lagerverwaltung/
WORKDIR /src/Lagerverwaltung
RUN dotnet restore

# Kopiere den restlichen Code
COPY . ./Lagerverwaltung/
WORKDIR /src/Lagerverwaltung

# Konfiguration + Secrets
COPY ./appsettings.json ./appsettings.json
COPY ./Secrets/ ./Secrets/

# Build & Publish
RUN dotnet publish -c Release -o /app/publish

# --- Final Image ---
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY lagerverwaltung.db ./lagerverwaltung.db
ENTRYPOINT ["dotnet", "backend.dll"]
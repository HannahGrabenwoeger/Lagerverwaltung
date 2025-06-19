# --- Base Runtime ---
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
    WORKDIR /app
    
    # --- Build Stage ---
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    
    # Kopiere nur die Projektdateien
    COPY ./backend/backend.csproj ./Lagerverwaltung_backend/backend/
    COPY ./Lagerverwaltung.sln ./
    
    # Restore NuGet-Pakete
    RUN dotnet restore
    
    # Kopiere den restlichen Code
    COPY ./backend/ ./Lagerverwaltung_backend/backend/
    WORKDIR /src/Lagerverwaltung_backend/backend
    
    # Build & Publish
    RUN dotnet publish -c Release -o /app/publish
    
    # --- Final Image ---
    FROM base AS final
    WORKDIR /app

    
    COPY --from=build /app/publish .
    ENTRYPOINT ["dotnet", "backend.dll"]
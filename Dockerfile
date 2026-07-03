# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore as a separate, cacheable layer (only re-runs when the csproj changes).
# The bracketed COPY form is required because the path contains spaces.
COPY ["ANKAVERA İÇ GİYİM/ANKAVERA İÇ GİYİM.csproj", "ANKAVERA İÇ GİYİM/"]
RUN dotnet restore "ANKAVERA İÇ GİYİM/ANKAVERA İÇ GİYİM.csproj"

# Copy the rest of the source and publish a Release build.
# UseAppHost=false skips the native launcher — the app runs via `dotnet <dll>`.
COPY . .
RUN dotnet publish "ANKAVERA İÇ GİYİM/ANKAVERA İÇ GİYİM.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Kestrel listens on 8080 inside the container (.NET 8 default for the aspnet image).
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Copy only the published output; run as the built-in non-root `app` user.
COPY --from=build --chown=app:app /app/publish .
USER app

ENTRYPOINT ["dotnet", "ANKAVERA_SİTESİ.dll"]

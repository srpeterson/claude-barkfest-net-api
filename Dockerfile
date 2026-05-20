# syntax=docker/dockerfile:1

# ---------- runtime base ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy props files first — TargetFramework and package versions are defined here
COPY ["Directory.Packages.props", "."]
COPY ["src/Directory.Build.props", "src/"]

# Copy project files so Docker cache is reused when only source changes
COPY ["src/Barkfest.API/Barkfest.API.csproj",                     "src/Barkfest.API/"]
COPY ["src/Barkfest.Application/Barkfest.Application.csproj",     "src/Barkfest.Application/"]
COPY ["src/Barkfest.Domain/Barkfest.Domain.csproj",               "src/Barkfest.Domain/"]
COPY ["src/Barkfest.Infrastructure/Barkfest.Infrastructure.csproj","src/Barkfest.Infrastructure/"]
COPY ["src/Barkfest.Persistence/Barkfest.Persistence.csproj",     "src/Barkfest.Persistence/"]
COPY ["src/Barkfest.ServiceDefaults/Barkfest.ServiceDefaults.csproj","src/Barkfest.ServiceDefaults/"]

RUN dotnet restore "src/Barkfest.API/Barkfest.API.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "src/Barkfest.API/Barkfest.API.csproj" \
    --configuration Release \
    --no-restore

# ---------- publish ----------
FROM build AS publish
RUN dotnet publish "src/Barkfest.API/Barkfest.API.csproj" \
    --configuration Release \
    --no-build \
    --output /app/publish

# ---------- final ----------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Barkfest.API.dll"]

# ── Build API ──────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /src
COPY api/ ./
RUN dotnet restore RpsLs.sln
RUN dotnet publish RpsLs.Api/RpsLs.Api.csproj -c Release -o /app/api

# ── Build UI ───────────────────────────────────────────────────────────────────
FROM node:20-alpine AS ui-build
WORKDIR /ui
COPY ui/package*.json ./
RUN npm ci
COPY ui/ ./
# Point UI at the API served from the same origin in production
ENV VITE_API_BASE=""
RUN npm run build

# ── Runtime ────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=api-build /app/api ./
# Serve the built React app from wwwroot
COPY --from=ui-build /ui/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000
ENTRYPOINT ["dotnet", "RpsLs.Api.dll"]

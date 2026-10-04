# ============================================================
# ETAPA 1: BUILD
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Instituto.AD/Instituto.AD.csproj Instituto.AD/
COPY Instituto.BR/Instituto.BR.csproj Instituto.BR/
COPY Instituto.MinimalAPI/Instituto.MinimalAPI.csproj Instituto.MinimalAPI/
RUN dotnet restore Instituto.MinimalAPI/Instituto.MinimalAPI.csproj

COPY Instituto.AD/ Instituto.AD/
COPY Instituto.BR/ Instituto.BR/
COPY Instituto.MinimalAPI/ Instituto.MinimalAPI/

WORKDIR /src/Instituto.MinimalAPI
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ============================================================
# ETAPA 2: RUNTIME
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
ENTRYPOINT ["dotnet", "Instituto.MinimalAPI.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/LogPort.Api/LogPort.Api.csproj", "src/LogPort.Api/"]
COPY ["src/LogPort.Core/LogPort.Core.csproj", "src/LogPort.Core/"]
COPY ["src/LogPort.ElasticSearch/LogPort.ElasticSearch.csproj", "src/LogPort.ElasticSearch/"]
COPY ["src/LogPort.Postgres/LogPort.Postgres.csproj", "src/LogPort.Postgres/"]
RUN dotnet restore "src/LogPort.Api/LogPort.Api.csproj"
COPY . .
WORKDIR "/src/src/LogPort.Api"
RUN dotnet build "./LogPort.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LogPort.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogPort.Api.dll"]

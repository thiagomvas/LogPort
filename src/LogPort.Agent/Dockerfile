FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM node:20 AS ui-build
WORKDIR /src/LogPort.UI
COPY src/LogPort.UI/package*.json ./
RUN npm ci --verbose
COPY src/LogPort.UI .
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/LogPort.Agent/LogPort.Agent.csproj", "src/LogPort.Agent/"]
COPY ["src/LogPort.Core/LogPort.Core.csproj", "src/LogPort.Core/"]
COPY ["src/LogPort.Internal.Common/LogPort.Internal.Common.csproj", "src/LogPort.Internal.Common/"]
COPY ["src/LogPort.Internal.Postgres/LogPort.Internal.Postgres.csproj", "src/LogPort.Internal.Postgres/"]
RUN dotnet restore "src/LogPort.Agent/LogPort.Agent.csproj"
COPY . .
WORKDIR "/src/src/LogPort.Agent"
RUN dotnet build "./LogPort.Agent.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LogPort.Agent.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=ui-build /src/LogPort.UI/dist ./wwwroot
ENTRYPOINT ["dotnet", "LogPort.Agent.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DockerUpdater.Api/DockerUpdater.Api.csproj", "DockerUpdater.Api/"]
RUN dotnet restore "DockerUpdater.Api/DockerUpdater.Api.csproj"
COPY . .
WORKDIR "/src/DockerUpdater.Api"
RUN dotnet build "./DockerUpdater.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DockerUpdater.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DockerUpdater.Api.dll"]

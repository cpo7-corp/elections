FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["elections.api/Elections.Api/Elections.Api.csproj", "elections.api/Elections.Api/"]
RUN dotnet restore "elections.api/Elections.Api/Elections.Api.csproj"
COPY . .
WORKDIR "/src/elections.api/Elections.Api"
RUN dotnet build "Elections.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Elections.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Elections.Api.dll"]
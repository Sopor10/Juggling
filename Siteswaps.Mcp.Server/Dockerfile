FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "/"]
COPY ["Directory.Packages.props", "/"]
COPY ["Siteswaps.Mcp.Server/Siteswaps.Mcp.Server.csproj", "Siteswaps.Mcp.Server/"]
COPY ["Generator/Siteswaps.Generator.Core/Siteswaps.Generator.Core.csproj", "Generator/Siteswaps.Generator.Core/"]
COPY ["Siteswap.Details/Siteswap.Details.csproj", "Siteswap.Details/"]
RUN dotnet restore "Siteswaps.Mcp.Server/Siteswaps.Mcp.Server.csproj"
COPY . .
WORKDIR "/src/Siteswaps.Mcp.Server"
RUN dotnet build "./Siteswaps.Mcp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Siteswaps.Mcp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Siteswaps.Mcp.Server.dll"]

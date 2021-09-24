FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Siteswaps.ConsoleApp/Siteswaps.ConsoleApp.csproj", "Siteswaps.ConsoleApp/"]
RUN dotnet restore "Siteswaps.ConsoleApp/Siteswaps.ConsoleApp.csproj"
COPY . .
WORKDIR "/src/Siteswaps.ConsoleApp"
RUN dotnet build "Siteswaps.ConsoleApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Siteswaps.ConsoleApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Siteswaps.ConsoleApp.dll"]

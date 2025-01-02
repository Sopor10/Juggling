FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Webassembly/Webassembly.csproj", "Webassembly/"]
RUN dotnet restore "Webassembly/Webassembly.csproj"
COPY . .
WORKDIR "/src/Webassembly"
RUN dotnet build "Webassembly.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Webassembly.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Webassembly.dll"]

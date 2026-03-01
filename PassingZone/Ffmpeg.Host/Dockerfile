FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y \
    imagemagick \
    ffmpeg \
    melt \
    bc \
    fontconfig \
    dos2unix \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "/"]
COPY ["Directory.Packages.props", "/"]
COPY ["PassingZone/Ffmpeg.Host/Ffmpeg.Host.csproj", "PassingZone/Ffmpeg.Host/"]
RUN dotnet restore "PassingZone/Ffmpeg.Host/Ffmpeg.Host.csproj"
COPY . .
WORKDIR "/src/PassingZone/Ffmpeg.Host"
RUN dotnet build "Ffmpeg.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Ffmpeg.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN find ./Reference-Adrian -type f -name "*.sh" -exec dos2unix {} + && chmod +x ./Reference-Adrian/render.sh

# Install fonts
RUN mkdir -p ~/.fonts && \
    cp ./Reference-Adrian/common/*.ttf ~/.fonts && \
    fc-cache -f -v

ENTRYPOINT ["dotnet", "Ffmpeg.Host.dll"]

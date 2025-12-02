FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/FitTrackPro.API/FitTrackPro.API.csproj", "src/FitTrackPro.API/"]
COPY ["src/FitTrackPro.Application/FitTrackPro.Application.csproj", "src/FitTrackPro.Application/"]
COPY ["src/FitTrackPro.Domain/FitTrackPro.Domain.csproj", "src/FitTrackPro.Domain/"]
COPY ["src/FitTrackPro.Infrastructure/FitTrackPro.Infrastructure.csproj", "src/FitTrackPro.Infrastructure/"]

RUN dotnet restore "src/FitTrackPro.API/FitTrackPro.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/FitTrackPro.API"
RUN dotnet build "FitTrackPro.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FitTrackPro.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/uploads

ENTRYPOINT ["dotnet", "FitTrackPro.API.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY ["GatewayMS.API/GatewayMS.API.csproj", "GatewayMS.API/"]
RUN dotnet restore "GatewayMS.API/GatewayMS.API.csproj"
COPY . .
WORKDIR "/src/GatewayMS.API"
RUN dotnet build "GatewayMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GatewayMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GatewayMS.API.dll"]

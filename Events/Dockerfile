FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Events/EventsMS.API/EventsMS.API.csproj", "Events/EventsMS.API/"]
COPY ["Events/EventsMS.Application/EventsMS.Application.csproj", "Events/EventsMS.Application/"]
COPY ["Events/EventsMS.Domain/EventsMS.Domain.csproj", "Events/EventsMS.Domain/"]
COPY ["Events/EventsMS.Infrastructure/EventsMS.Infrastructure.csproj", "Events/EventsMS.Infrastructure/"]
COPY ["Events/EventsMS.Shared/EventsMS.Shared.csproj", "Events/EventsMS.Shared/"]

RUN dotnet restore "Events/EventsMS.API/EventsMS.API.csproj"

COPY Events/ Events/

WORKDIR "/src/Events/EventsMS.API"
RUN dotnet build "EventsMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventsMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventsMS.API.dll"]

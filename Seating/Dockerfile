FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Seating/SeatingMS.API/SeatingMS.API.csproj", "Seating/SeatingMS.API/"]
COPY ["Seating/SeatingMS.Application/SeatingMS.Application.csproj", "Seating/SeatingMS.Application/"]
COPY ["Seating/SeatingMS.Domain/SeatingMS.Domain.csproj", "Seating/SeatingMS.Domain/"]
COPY ["Seating/SeatingMS.Infrastructure/SeatingMS.Infrastructure.csproj", "Seating/SeatingMS.Infrastructure/"]
COPY ["Seating/SeatingMS.Shared/SeatingMS.Shared.csproj", "Seating/SeatingMS.Shared/"]
COPY ["Booking/BookingMS.Shared/BookingMS.Shared.csproj", "Booking/BookingMS.Shared/"]
COPY ["Events/EventsMS.Shared/EventsMS.Shared.csproj", "Events/EventsMS.Shared/"]

RUN dotnet restore "Seating/SeatingMS.API/SeatingMS.API.csproj"

COPY Seating/ Seating/
COPY Booking/BookingMS.Shared/ Booking/BookingMS.Shared/
COPY Events/EventsMS.Shared/ Events/EventsMS.Shared/

WORKDIR "/src/Seating/SeatingMS.API"
RUN dotnet build "SeatingMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SeatingMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SeatingMS.API.dll"]

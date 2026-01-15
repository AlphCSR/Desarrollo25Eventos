FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Booking/BookingMS.API/BookingMS.API.csproj", "Booking/BookingMS.API/"]
COPY ["Booking/BookingMS.Application/BookingMS.Application.csproj", "Booking/BookingMS.Application/"]
COPY ["Booking/BookingMS.Domain/BookingMS.Domain.csproj", "Booking/BookingMS.Domain/"]
COPY ["Booking/BookingMS.Infrastructure/BookingMS.Infrastructure.csproj", "Booking/BookingMS.Infrastructure/"]
COPY ["Booking/BookingMS.Shared/BookingMS.Shared.csproj", "Booking/BookingMS.Shared/"]
COPY ["Seating/SeatingMS.Shared/SeatingMS.Shared.csproj", "Seating/SeatingMS.Shared/"]
COPY ["Events/EventsMS.Shared/EventsMS.Shared.csproj", "Events/EventsMS.Shared/"]

RUN dotnet restore "Booking/BookingMS.API/BookingMS.API.csproj"

COPY Booking/ Booking/
COPY Seating/SeatingMS.Shared/ Seating/SeatingMS.Shared/
COPY Events/EventsMS.Shared/ Events/EventsMS.Shared/

WORKDIR "/src/Booking/BookingMS.API"
RUN dotnet build "BookingMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookingMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookingMS.API.dll"]

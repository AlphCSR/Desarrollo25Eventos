FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Reports/ReportsMS.API/ReportsMS.API.csproj", "Reports/ReportsMS.API/"]
COPY ["Reports/ReportsMS.Application/ReportsMS.Application.csproj", "Reports/ReportsMS.Application/"]
COPY ["Reports/ReportsMS.Core/ReportsMS.Core.csproj", "Reports/ReportsMS.Core/"]
COPY ["Reports/ReportsMS.Domain/ReportsMS.Domain.csproj", "Reports/ReportsMS.Domain/"]
COPY ["Reports/ReportsMS.Infrastructure/ReportsMS.Infrastructure.csproj", "Reports/ReportsMS.Infrastructure/"]
COPY ["Reports/ReportsMS.Shared/ReportsMS.Shared.csproj", "Reports/ReportsMS.Shared/"]

COPY ["Booking/BookingMS.Shared/BookingMS.Shared.csproj", "Booking/BookingMS.Shared/"]
COPY ["Payments/PaymentsMS.Shared/PaymentsMS.Shared.csproj", "Payments/PaymentsMS.Shared/"]
COPY ["Events/EventsMS.Shared/EventsMS.Shared.csproj", "Events/EventsMS.Shared/"]

RUN dotnet restore "Reports/ReportsMS.API/ReportsMS.API.csproj"

COPY Reports/ Reports/
COPY Booking/BookingMS.Shared/ Booking/BookingMS.Shared/
COPY Payments/PaymentsMS.Shared/ Payments/PaymentsMS.Shared/
COPY Events/EventsMS.Shared/ Events/EventsMS.Shared/

WORKDIR "/src/Reports/ReportsMS.API"
RUN dotnet build "ReportsMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReportsMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReportsMS.API.dll"]

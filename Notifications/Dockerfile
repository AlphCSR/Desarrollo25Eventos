FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Notifications/NotificationsMS.API/NotificationsMS.API.csproj", "Notifications/NotificationsMS.API/"]
COPY ["Notifications/NotificationsMS.Application/NotificationsMS.Application.csproj", "Notifications/NotificationsMS.Application/"]
COPY ["Notifications/NotificationsMS.Domain/NotificationsMS.Domain.csproj", "Notifications/NotificationsMS.Domain/"]
COPY ["Notifications/NotificationsMS.Infrastructure/NotificationsMS.Infrastructure.csproj", "Notifications/NotificationsMS.Infrastructure/"]
COPY ["Notifications/NotificationsMS.Shared/NotificationsMS.Shared.csproj", "Notifications/NotificationsMS.Shared/"]
COPY ["Booking/BookingMS.Shared/BookingMS.Shared.csproj", "Booking/BookingMS.Shared/"]
COPY ["Payments/PaymentsMS.Shared/PaymentsMS.Shared.csproj", "Payments/PaymentsMS.Shared/"]
COPY ["Services/ServicesMS.Shared/ServicesMS.Shared.csproj", "Services/ServicesMS.Shared/"]

RUN dotnet restore "Notifications/NotificationsMS.API/NotificationsMS.API.csproj"

COPY Notifications/ Notifications/
COPY Booking/BookingMS.Shared/ Booking/BookingMS.Shared/
COPY Payments/PaymentsMS.Shared/ Payments/PaymentsMS.Shared/
COPY Services/ServicesMS.Shared/ Services/ServicesMS.Shared/

WORKDIR "/src/Notifications/NotificationsMS.API"
RUN dotnet build "NotificationsMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationsMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationsMS.API.dll"]

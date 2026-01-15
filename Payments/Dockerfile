FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Payments/PaymentsMS.API/PaymentsMS.API.csproj", "Payments/PaymentsMS.API/"]
COPY ["Payments/PaymentsMS.Application/PaymentsMS.Application.csproj", "Payments/PaymentsMS.Application/"]
COPY ["Payments/PaymentsMS.Domain/PaymentsMS.Domain.csproj", "Payments/PaymentsMS.Domain/"]
COPY ["Payments/PaymentsMS.Infrastructure/PaymentsMS.Infrastructure.csproj", "Payments/PaymentsMS.Infrastructure/"]
COPY ["Payments/PaymentsMS.Shared/PaymentsMS.Shared.csproj", "Payments/PaymentsMS.Shared/"]
COPY ["Booking/BookingMS.Shared/BookingMS.Shared.csproj", "Booking/BookingMS.Shared/"]

RUN dotnet restore "Payments/PaymentsMS.API/PaymentsMS.API.csproj"

COPY Payments/ Payments/
COPY Booking/BookingMS.Shared/ Booking/BookingMS.Shared/

WORKDIR "/src/Payments/PaymentsMS.API"
RUN dotnet build "PaymentsMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentsMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentsMS.API.dll"]

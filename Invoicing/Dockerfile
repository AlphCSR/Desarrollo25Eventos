FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Invoicing/InvoicingMS.API/InvoicingMS.API.csproj", "Invoicing/InvoicingMS.API/"]
COPY ["Invoicing/InvoicingMS.Application/InvoicingMS.Application.csproj", "Invoicing/InvoicingMS.Application/"]
COPY ["Invoicing/InvoicingMS.Domain/InvoicingMS.Domain.csproj", "Invoicing/InvoicingMS.Domain/"]
COPY ["Invoicing/InvoicingMS.Infrastructure/InvoicingMS.Infrastructure.csproj", "Invoicing/InvoicingMS.Infrastructure/"]
COPY ["Invoicing/InvoicingMS.Shared/InvoicingMS.Shared.csproj", "Invoicing/InvoicingMS.Shared/"]
COPY ["Booking/BookingMS.Shared/BookingMS.Shared.csproj", "Booking/BookingMS.Shared/"]

RUN dotnet restore "Invoicing/InvoicingMS.API/InvoicingMS.API.csproj"

COPY Invoicing/ Invoicing/
COPY Booking/BookingMS.Shared/ Booking/BookingMS.Shared/

WORKDIR "/src/Invoicing/InvoicingMS.API"
RUN dotnet build "InvoicingMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InvoicingMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/invoices
ENTRYPOINT ["dotnet", "InvoicingMS.API.dll"]

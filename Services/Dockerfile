FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Services/ServicesMS.API/ServicesMS.API.csproj", "Services/ServicesMS.API/"]
COPY ["Services/ServicesMS.Application/ServicesMS.Application.csproj", "Services/ServicesMS.Application/"]
COPY ["Services/ServicesMS.Domain/ServicesMS.Domain.csproj", "Services/ServicesMS.Domain/"]
COPY ["Services/ServicesMS.Infrastructure/ServicesMS.Infrastructure.csproj", "Services/ServicesMS.Infrastructure/"]
COPY ["Services/ServicesMS.Shared/ServicesMS.Shared.csproj", "Services/ServicesMS.Shared/"]
COPY ["Booking/BookingMS.Shared/BookingMS.Shared.csproj", "Booking/BookingMS.Shared/"]

RUN dotnet restore "Services/ServicesMS.API/ServicesMS.API.csproj"

COPY Services/ Services/
COPY Booking/BookingMS.Shared/ Booking/BookingMS.Shared/

WORKDIR "/src/Services/ServicesMS.API"
RUN dotnet build "ServicesMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServicesMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServicesMS.API.dll"]

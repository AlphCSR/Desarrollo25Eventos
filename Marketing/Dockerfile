FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Marketing/MarketingMS.API/MarketingMS.API.csproj", "Marketing/MarketingMS.API/"]
COPY ["Marketing/MarketingMS.Application/MarketingMS.Application.csproj", "Marketing/MarketingMS.Application/"]
COPY ["Marketing/MarketingMS.Domain/MarketingMS.Domain.csproj", "Marketing/MarketingMS.Domain/"]
COPY ["Marketing/MarketingMS.Infrastructure/MarketingMS.Infrastructure.csproj", "Marketing/MarketingMS.Infrastructure/"]
COPY ["Marketing/MarketingMS.Shared/MarketingMS.Shared.csproj", "Marketing/MarketingMS.Shared/"]
COPY ["Booking/BookingMS.Shared/BookingMS.Shared.csproj", "Booking/BookingMS.Shared/"]
COPY ["Users/UsersMS.Shared/UsersMS.Shared.csproj", "Users/UsersMS.Shared/"]

RUN dotnet restore "Marketing/MarketingMS.API/MarketingMS.API.csproj"

COPY Marketing/ Marketing/
COPY Booking/BookingMS.Shared/ Booking/BookingMS.Shared/
COPY Users/UsersMS.Shared/ Users/UsersMS.Shared/

WORKDIR "/src/Marketing/MarketingMS.API"
RUN dotnet build "MarketingMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarketingMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MarketingMS.API.dll"]

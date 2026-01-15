FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Users/UsersMS.API/UsersMS.API.csproj", "Users/UsersMS.API/"]
COPY ["Users/UsersMS.Application/UsersMS.Application.csproj", "Users/UsersMS.Application/"]
COPY ["Users/UsersMS.Domain/UsersMS.Domain.csproj", "Users/UsersMS.Domain/"]
COPY ["Users/UsersMS.Infrastructure/UsersMS.Infrastructure.csproj", "Users/UsersMS.Infrastructure/"]
COPY ["Users/UsersMS.Shared/UsersMS.Shared.csproj", "Users/UsersMS.Shared/"]
COPY ["Events/EventsMS.Shared/EventsMS.Shared.csproj", "Events/EventsMS.Shared/"]
COPY ["Booking/BookingMS.Shared/BookingMS.Shared.csproj", "Booking/BookingMS.Shared/"]
COPY ["Services/ServicesMS.Shared/ServicesMS.Shared.csproj", "Services/ServicesMS.Shared/"]

RUN dotnet restore "Users/UsersMS.API/UsersMS.API.csproj"

COPY Users/ Users/
COPY Events/EventsMS.Shared/ Events/EventsMS.Shared/
COPY Booking/BookingMS.Shared/ Booking/BookingMS.Shared/
COPY Services/ServicesMS.Shared/ Services/ServicesMS.Shared/

WORKDIR "/src/Users/UsersMS.API"
RUN dotnet build "UsersMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UsersMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UsersMS.API.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Community/CommunityMS.API/CommunityMS.API.csproj", "Community/CommunityMS.API/"]
COPY ["Community/CommunityMS.Application/CommunityMS.Application.csproj", "Community/CommunityMS.Application/"]
COPY ["Community/CommunityMS.Domain/CommunityMS.Domain.csproj", "Community/CommunityMS.Domain/"]
COPY ["Community/CommunityMS.Infrastructure/CommunityMS.Infrastructure.csproj", "Community/CommunityMS.Infrastructure/"]
COPY ["Community/CommunityMS.Shared/CommunityMS.Shared.csproj", "Community/CommunityMS.Shared/"]

RUN dotnet restore "Community/CommunityMS.API/CommunityMS.API.csproj"

COPY Community/ Community/

WORKDIR "/src/Community/CommunityMS.API"
RUN dotnet build "CommunityMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommunityMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommunityMS.API.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache icu-libs
WORKDIR /src

COPY ["Survey/SurveyMS.API/SurveyMS.API.csproj", "Survey/SurveyMS.API/"]
COPY ["Survey/SurveyMS.Application/SurveyMS.Application.csproj", "Survey/SurveyMS.Application/"]
COPY ["Survey/SurveyMS.Domain/SurveyMS.Domain.csproj", "Survey/SurveyMS.Domain/"]
COPY ["Survey/SurveyMS.Infrastructure/SurveyMS.Infrastructure.csproj", "Survey/SurveyMS.Infrastructure/"]
COPY ["Survey/SurveyMS.Shared/SurveyMS.Shared.csproj", "Survey/SurveyMS.Shared/"]

RUN dotnet restore "Survey/SurveyMS.API/SurveyMS.API.csproj"

COPY Survey/ Survey/

WORKDIR "/src/Survey/SurveyMS.API"
RUN dotnet build "SurveyMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SurveyMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SurveyMS.API.dll"]

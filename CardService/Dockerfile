FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CardService/Api/CardService.Api.csproj", "CardService/Api/"]
COPY ["CardService/Modules/Cards/Domain/Cards.Domain.csproj", "CardService/Modules/Cards/Domain/"]
COPY ["CardService/Modules/Cards/Application/Cards.Application.csproj", "CardService/Modules/Cards/Application/"]
COPY ["CardService/Modules/Cards/Infrastructure/Cards.Infrastructure.csproj", "CardService/Modules/Cards/Infrastructure/"]
COPY ["CardService/Modules/Cards/Api/Cards.Api.csproj", "CardService/Modules/Cards/Api/"]
RUN dotnet restore "CardService/Api/CardService.Api.csproj"
COPY . .
WORKDIR "/src/CardService/Api"
RUN dotnet build "./CardService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CardService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CardService.Api.dll"]

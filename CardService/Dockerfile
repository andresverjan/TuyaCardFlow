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
COPY ["CardService/Modules/Payments/Domain/Payments.Domain.csproj", "CardService/Modules/Payments/Domain/"]
COPY ["CardService/Modules/Payments/Application/Payments.Application.csproj", "CardService/Modules/Payments/Application/"]
COPY ["CardService/Modules/Payments/Infrastructure/Payments.Infrastructure.csproj", "CardService/Modules/Payments/Infrastructure/"]
COPY ["CardService/Modules/Payments/Api/Payments.Api.csproj", "CardService/Modules/Payments/Api/"]
COPY ["CardService/Modules/Users/Domain/Users.Domain.csproj", "CardService/Modules/Users/Domain/"]
COPY ["CardService/Modules/Users/Application/Users.Application.csproj", "CardService/Modules/Users/Application/"]
COPY ["CardService/Modules/Users/Infrastructure/Users.Infrastructure.csproj", "CardService/Modules/Users/Infrastructure/"]
COPY ["CardService/Modules/Users/Api/Users.Api.csproj", "CardService/Modules/Users/Api/"]
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

FROM node:24-alpine AS client
WORKDIR /client
COPY CardServiceFront/ClientApp/package*.json ./
RUN npm ci
COPY CardServiceFront/ClientApp/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY CardServiceFront/CardServiceFront.csproj CardServiceFront/
RUN dotnet restore CardServiceFront/CardServiceFront.csproj
COPY CardServiceFront/ CardServiceFront/
COPY --from=client /wwwroot/ CardServiceFront/wwwroot/
RUN dotnet publish CardServiceFront/CardServiceFront.csproj -c Release -o /app/publish -p:BuildClientApp=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
USER $APP_UID
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CardServiceFront.dll"]

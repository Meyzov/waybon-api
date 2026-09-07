# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Waybon.Api/Waybon.Api.csproj src/Waybon.Api/
COPY src/Waybon.Application/Waybon.Application.csproj src/Waybon.Application/
COPY src/Waybon.Domain/Waybon.Domain.csproj src/Waybon.Domain/
COPY src/Waybon.Infrastructure/Waybon.Infrastructure.csproj src/Waybon.Infrastructure/

RUN dotnet restore src/Waybon.Api/Waybon.Api.csproj

COPY src/ src/
RUN dotnet publish src/Waybon.Api/Waybon.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet Waybon.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]

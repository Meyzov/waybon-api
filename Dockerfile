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

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "Waybon.Api.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./ ./

EXPOSE 5000
EXPOSE 5002

ENTRYPOINT ["dotnet", "Upflux-WebService.dll"]

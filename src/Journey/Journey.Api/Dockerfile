# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Journey/Journey.Api/Journey.Api.csproj", "Journey/Journey.Api/"]
COPY ["src/Journey/Journey.Application/Journey.Application.csproj", "Journey/Journey.Application/"]
COPY ["src/Journey/Journey.Contracts/Journey.Contracts.csproj", "Journey/Journey.Contracts/"]
COPY ["src/Journey/Journey.Domain/Journey.Domain.csproj", "Journey/Journey.Domain/"]
COPY ["src/Journey/Journey.Infrastructure/Journey.Infrastructure.csproj", "Journey/Journey.Infrastructure/"]
RUN dotnet restore "./Journey/Journey.Api/Journey.Api.csproj"
COPY . .
WORKDIR "/src/src/Journey/Journey.Api"
RUN dotnet build "./Journey.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./Journey.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Journey.Api.dll"]
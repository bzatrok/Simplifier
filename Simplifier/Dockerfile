FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

# Install Node.js
RUN apt update && apt install -y curl software-properties-common && \
    curl -sL https://deb.nodesource.com/setup_18.x | bash - && \
    apt install -y nodejs

WORKDIR /src
COPY ["Simplifier/Simplifier.csproj", "Simplifier/"]
RUN dotnet restore "Simplifier/Simplifier.csproj"
COPY . .
WORKDIR "/src/Simplifier"
RUN dotnet build "Simplifier.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Simplifier.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Simplifier.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# New stage for handling Node.js operations
FROM node:18 AS node_build
WORKDIR /web
# Copy only the necessary files for yarn install
COPY ["Simplifier/package.json", "Simplifier/yarn.lock", "Simplifier/tailwind.config.js", "./"]
COPY ["Simplifier/Styles/globals.css", "./Styles/"]
RUN yarn install
RUN yarn tailwind-build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG SKIP_NPM_INSTALL=true
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Simplifier/Simplifier.csproj", "Simplifier/"]
RUN dotnet restore "Simplifier/Simplifier.csproj"
COPY . .
WORKDIR "/src/Simplifier"
COPY --from=node_build /web/wwwroot/css ./wwwroot/css/
RUN dotnet build "Simplifier.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Simplifier.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Simplifier.dll"]

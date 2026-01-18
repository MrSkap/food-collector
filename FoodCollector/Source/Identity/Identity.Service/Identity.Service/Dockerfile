FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Source/Identity/Identity.Service/Identity.Service/Identity.Service.csproj", "Source/Identity/Identity.Service/Identity.Service/"]
RUN dotnet restore "Source/Identity/Identity.Service/Identity.Service/Identity.Service.csproj"
COPY . .
WORKDIR "/src/Source/Identity/Identity.Service/Identity.Service"
RUN dotnet build "./Identity.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Identity.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.Service.dll"]

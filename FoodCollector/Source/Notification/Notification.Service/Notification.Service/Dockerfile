FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Source/Notification/Notification.Service/Notification.Service/Notification.Service.csproj", "Source/Notification/Notification.Service/Notification.Service/"]
RUN dotnet restore "Source/Notification/Notification.Service/Notification.Service/Notification.Service.csproj"
COPY . .
WORKDIR "/src/Source/Notification/Notification.Service/Notification.Service"
RUN dotnet build "./Notification.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Notification.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.Service.dll"]

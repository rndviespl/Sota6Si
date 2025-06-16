# Базовый образ для runtime .NET 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Сборка приложения
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Sota6Si/Sota6Si.csproj", "Sota6Si/"]
RUN dotnet restore "Sota6Si/Sota6Si.csproj"
COPY . .
RUN dotnet build "Sota6Si/Sota6Si.csproj" -c Release -o /app/build

# Публикация приложения
FROM build AS publish
RUN dotnet publish "Sota6Si/Sota6Si.csproj" -c Release -o /app/publish

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sota6Si.dll"]

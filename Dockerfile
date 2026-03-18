FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de solución y proyecto
COPY ["arroyoSeco.sln", "./"]
COPY ["arroyoSeco/arroyoSeco.API.csproj", "arroyoSeco/"]
COPY ["arroyoSeco.Application/arroyoSeco.Application.csproj", "arroyoSeco.Application/"]
COPY ["arroyoSeco.Domain/arroyoSeco.Domain.csproj", "arroyoSeco.Domain/"]
COPY ["arroyoSeco.Infrastructure/arroyoSeco.Infrastructure.csproj", "arroyoSeco.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "arroyoSeco/arroyoSeco.API.csproj"

# Copiar todo el código
COPY . .

# Compilar y publicar
WORKDIR "/src/arroyoSeco"
RUN dotnet publish "arroyoSeco.API.csproj" -c Release -o /app/publish

# Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Variables de entorno por defecto
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "arroyoSeco.API.dll"]

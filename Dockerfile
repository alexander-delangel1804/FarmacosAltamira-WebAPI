# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de configuración de paquetes y solución
COPY ["Directory.Packages.props", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["FarmacosAltamira.sln", "./"]

# Copiar el proyecto de la API y restaurar dependencias
COPY ["src/FarmacosAltamira.Api/FarmacosAltamira.Api.csproj", "src/FarmacosAltamira.Api/"]
COPY ["src/FarmacosAltamira.Application/FarmacosAltamira.Application.csproj", "src/FarmacosAltamira.Application/"]
COPY ["src/FarmacosAltamira.Domain/FarmacosAltamira.Domain.csproj", "src/FarmacosAltamira.Domain/"]
COPY ["src/FarmacosAltamira.Contracts/FarmacosAltamira.Contracts.csproj", "src/FarmacosAltamira.Contracts/"]
COPY ["src/FarmacosAltamira.Infrastructure/FarmacosAltamira.Infrastructure.csproj", "src/FarmacosAltamira.Infrastructure/"]

RUN dotnet restore "FarmacosAltamira.sln"

# Copiar todo el código fuente
COPY . .

# Publicar la API
RUN dotnet publish "src/FarmacosAltamira.Api/FarmacosAltamira.Api.csproj" -c Release -o /app/publish

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_HTTP_PORTS=10000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FarmacosAltamira.Api.dll"]
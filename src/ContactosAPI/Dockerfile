# ---- Etapa 1: construir (imagen grande con el SDK) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Se copia solo el .csproj primero para aprovechar la caché de Docker:
# si no cambian los paquetes, el restore no se repite.
COPY ContactosAPI.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---- Etapa 2: ejecutar (imagen liviana, solo runtime de ASP.NET) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Las imágenes de .NET 8+ ya escuchan en el puerto 8080 y traen el usuario "app" sin privilegios.
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "ContactosAPI.dll"]

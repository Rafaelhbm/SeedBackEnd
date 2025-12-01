# Estágio de Build (Usando SDK 9.0)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia e restaura
COPY ["SeedBackend_V1.csproj", "./"]
RUN dotnet restore "SeedBackend_V1.csproj"

# Compila
COPY . .
RUN dotnet publish "SeedBackend_V1.csproj" -c Release -o /app/publish

# Estágio Final 
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Porta do Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SeedBackend_V1.dll"]
# Usa a imagem oficial do .NET 8 para compilar o projeto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo de projeto e restaura as dependências
COPY ["SeedBackend_V1.csproj", "./"]
RUN dotnet restore "SeedBackend_V1.csproj"

# Copia o restante do código e compila
COPY . .
RUN dotnet publish "SeedBackend_V1.csproj" -c Release -o /app/publish

# Cria a imagem final para rodar o app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Define a porta que o Render usa (Obrigatório)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SeedBackend_V1.dll"]
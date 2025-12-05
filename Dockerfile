# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto
COPY ["FGC.Payments.Presentation/FGC.Payments.Presentation.csproj", "FGC.Payments.Presentation/"]
COPY ["FGC.Payments.Application/FGC.Payments.Application.csproj", "FGC.Payments.Application/"]
COPY ["FGC.Payments.Infrastructure/FGC.Payments.Infrastructure.csproj", "FGC.Payments.Infrastructure/"]
COPY ["FGC.Payments.Domain/FGC.Payments.Domain.csproj", "FGC.Payments.Domain/"]

# Restore
RUN dotnet restore "FGC.Payments.Presentation/FGC.Payments.Presentation.csproj"

# Copia todo o código
COPY . .

# Build
WORKDIR "/src/FGC.Payments.Presentation"
RUN dotnet build "FGC.Payments.Presentation.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FGC.Payments.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FGC.Payments.Presentation.dll"]
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["FGC.Payments.Presentation/FGC.Payments.Presentation.csproj", "FGC.Payments.Presentation/"]
COPY ["FGC.Payments.Application/FGC.Payments.Application.csproj", "FGC.Payments.Application/"]
COPY ["FGC.Payments.Infrastructure/FGC.Payments.Infrastructure.csproj", "FGC.Payments.Infrastructure/"]
COPY ["FGC.Payments.Domain/FGC.Payments.Domain.csproj", "FGC.Payments.Domain/"]

RUN dotnet restore "FGC.Payments.Presentation/FGC.Payments.Presentation.csproj"

COPY . .

WORKDIR "/src/FGC.Payments.Presentation"
RUN dotnet build "FGC.Payments.Presentation.csproj" -c Release -o /app/build --no-restore

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "FGC.Payments.Presentation.csproj" -c Release -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Stage 3: Runtime (Alpine)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

LABEL maintainer="FIAP Cloud Games" \
      version="2.0.0" \
      description="FGC Payments API - Microsserviço de Pagamentos"

RUN apk add --no-cache curl icu-libs

RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -D appuser

WORKDIR /app

COPY --from=publish /app/publish .

RUN chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "FGC.Payments.Presentation.dll"]
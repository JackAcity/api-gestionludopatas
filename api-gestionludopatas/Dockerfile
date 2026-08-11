# Fase 1 (spec despliegue-docker) — build multi-stage, mismo criterio de endurecimiento
# que reto_tecnico_backend_senior (C14): usuario no root, imagen runtime mínima.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/GestionLudopatas.Api/GestionLudopatas.Api.csproj src/GestionLudopatas.Api/
RUN dotnet restore src/GestionLudopatas.Api/GestionLudopatas.Api.csproj
COPY src/GestionLudopatas.Api/ src/GestionLudopatas.Api/
RUN dotnet publish src/GestionLudopatas.Api/GestionLudopatas.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd --gid 5000 gestionludopatas \
    && useradd --uid 5000 --gid gestionludopatas --shell /bin/false --no-create-home gestionludopatas
USER gestionludopatas

COPY --from=build --chown=gestionludopatas:gestionludopatas /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GestionLudopatas.Api.dll"]

# Fase 1 (spec despliegue-docker) — build multi-stage, mismo criterio de endurecimiento
# que reto_tecnico_backend_senior (C14): usuario no root, imagen runtime mínima.

FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:e1fc6e423f543119c406d24e2e687d67c569f18f04a37a8b0005d80ad0dcee80 AS build
WORKDIR /src
COPY src/GestionLudopatas.Api/GestionLudopatas.Api.csproj src/GestionLudopatas.Api/
RUN dotnet restore src/GestionLudopatas.Api/GestionLudopatas.Api.csproj
COPY src/GestionLudopatas.Api/ src/GestionLudopatas.Api/
RUN dotnet publish src/GestionLudopatas.Api/GestionLudopatas.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:207cc51496778557731c81ff670333d8ade4a4fec22768fd1be8e78474a84ecf AS runtime
WORKDIR /app

RUN groupadd --gid 5000 gestionludopatas \
    && useradd --uid 5000 --gid gestionludopatas --shell /bin/false --no-create-home gestionludopatas
USER gestionludopatas

COPY --from=build --chown=gestionludopatas:gestionludopatas /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GestionLudopatas.Api.dll"]

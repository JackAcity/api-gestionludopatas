using GestionLudopatas.Api.Endpoints;
using GestionLudopatas.Api.Documentacion;
using GestionLudopatas.Api.Infrastructure.Sql;
using GestionLudopatas.Api.Infrastructure.Vault;
using GestionLudopatas.Api.Middleware;
using GestionLudopatas.Api.Security;

var builder = WebApplication.CreateBuilder(args);

await builder.CargarSecretosSiHabilitadoAsync();

builder.Services.AddPersistenciaSql();
builder.Services.AddDocumentacionOpenApi();
builder.Services.AddExceptionHandler<ManejadorExcepcionesGlobal>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseTrazabilidad();
app.UseAutenticacionApiKey();
app.UseAllowlistDeIp();

app.MapHealthChecks("/health");
app.MapCorteEndpoints();
app.MapPendientesEndpoints();
app.MapDocumentacionOpenApiSoloDesarrollo();

app.Run();

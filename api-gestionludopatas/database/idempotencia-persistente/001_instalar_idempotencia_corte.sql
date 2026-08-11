/*
  Paquete DBA — NO lo ejecuta la API.

  Ejecutar con identidad DBA y variables explícitas, por ejemplo:
    sqlcmd -E -S <instancia> -v IdempotencyDatabase="<base_propia_api>" \
      BusinessDatabase="bd_autobot" ApiLoginName="app_api_p04_rw_automatizacion_gestion_ludopatas" \
      -i 001_instalar_idempotencia_corte.sql

  Precondiciones obligatorias:
  - $(IdempotencyDatabase) existe, es una base normal de la API y comparte instancia
    SQL Server con $(BusinessDatabase).
  - El login $(ApiLoginName) ya tiene EXECUTE en
    $(BusinessDatabase).dbo.SP_CORTE_Crear (D5).
  - DBA verificó que SP_CORTE_Crear participa correctamente en una transacción exterior.
  - No usar este diseño si una de las tablas involucradas es memory-optimized.

  Este script no crea una base de datos, no altera bd_autobot, dbo.Corte ni
  dbo.SP_CORTE_Crear, y no concede DML directo sobre la tabla de idempotencia.
*/

/*
  Las tres variables se reciben con sqlcmd -v (o se definen en SQLCMD mode antes de
  ejecutar este archivo). No usar :setvar aquí: tiene mayor precedencia que -v y podría
  reemplazar por accidente el nombre de base aprobado.
*/
IF N'$(IdempotencyDatabase)' = N''
    THROW 51060, 'Defina IdempotencyDatabase con la base propia aprobada para la API.', 1;

IF DB_ID(N'$(IdempotencyDatabase)') IS NULL
    THROW 51061, 'La base propia de idempotencia no existe.', 1;

IF DB_ID(N'$(BusinessDatabase)') IS NULL
    THROW 51062, 'La base de negocio no existe en esta instancia.', 1;
GO

USE [$(IdempotencyDatabase)];
GO

IF SCHEMA_ID(N'api') IS NULL
    EXEC(N'CREATE SCHEMA api AUTHORIZATION dbo;');
GO

IF OBJECT_ID(N'api.IdempotenciaCrearCorte', N'U') IS NULL
BEGIN
    CREATE TABLE api.IdempotenciaCrearCorte
    (
        IdempotencyKey     nvarchar(128) NOT NULL,
        PayloadFingerprint binary(32)    NOT NULL,
        CorteId            int           NOT NULL,
        HttpStatus         smallint      NOT NULL,
        CreadoUtc          datetime2(3)  NOT NULL,
        ExpiraUtc          datetime2(3)  NOT NULL,

        CONSTRAINT PK_IdempotenciaCrearCorte PRIMARY KEY CLUSTERED (IdempotencyKey),
        CONSTRAINT CK_IdempotenciaCrearCorte_HttpStatus CHECK (HttpStatus = 201),
        CONSTRAINT CK_IdempotenciaCrearCorte_Expiracion CHECK (ExpiraUtc > CreadoUtc)
    );

    CREATE INDEX IX_IdempotenciaCrearCorte_ExpiraUtc
        ON api.IdempotenciaCrearCorte (ExpiraUtc);
END;
GO

/*
  outcome:
    created  -> se creó un corte en esta transacción.
    replayed -> ya existía misma clave + huella; CorteId/HttpStatus son los originales.
    conflict -> misma clave con otra huella; CorteId/HttpStatus son NULL.

  HOLDLOCK mantiene una protección serializable sobre la clave (existente o ausente)
  hasta el commit. Nunca se persiste un estado "en curso": el lock protege la llamada
  al SP y la fila aparece únicamente junto con el corte confirmado.
*/
CREATE OR ALTER PROCEDURE api.SP_Corte_Crear_Idempotente
    @IdempotencyKey     nvarchar(128),
    @PayloadFingerprint binary(32),
    @TipoCorte          varchar(10),
    @FechaHoraCorte     datetime = NULL,
    @FechaHoraEjecucion datetime,
    @VigenciaHoras      int = 24
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF LEN(@IdempotencyKey) NOT BETWEEN 16 AND 128
        THROW 51063, 'Idempotency-Key debe tener entre 16 y 128 caracteres.', 1;

    IF @VigenciaHoras NOT BETWEEN 1 AND 168
        THROW 51064, 'La vigencia de idempotencia debe estar entre 1 y 168 horas.', 1;

    DECLARE
        @HuellaExistente binary(32),
        @CorteExistente int,
        @StatusExistente smallint,
        @ExpiraExistente datetime2(3),
        @CorteNuevo int,
        @AhoraUtc datetime2(3) = SYSUTCDATETIME();

    /* SP_CORTE_Crear devuelve OUTPUT INSERTED.id como un result set, no como parámetro. */
    DECLARE @ResultadoCorte TABLE (corte_id int NOT NULL);

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @HuellaExistente = PayloadFingerprint,
            @CorteExistente = CorteId,
            @StatusExistente = HttpStatus,
            @ExpiraExistente = ExpiraUtc
        FROM api.IdempotenciaCrearCorte WITH (UPDLOCK, HOLDLOCK)
        WHERE IdempotencyKey = @IdempotencyKey;

        IF @HuellaExistente IS NOT NULL AND @ExpiraExistente <= @AhoraUtc
        BEGIN
            DELETE FROM api.IdempotenciaCrearCorte
            WHERE IdempotencyKey = @IdempotencyKey;

            SELECT
                @HuellaExistente = NULL,
                @CorteExistente = NULL,
                @StatusExistente = NULL,
                @ExpiraExistente = NULL;
        END;

        IF @HuellaExistente IS NOT NULL
        BEGIN
            IF @HuellaExistente <> @PayloadFingerprint
            BEGIN
                COMMIT TRANSACTION;
                SELECT CAST(N'conflict' AS nvarchar(16)) AS outcome,
                       CAST(NULL AS int) AS corteId,
                       CAST(NULL AS smallint) AS httpStatus,
                       CAST(NULL AS bit) AS replayed;
                RETURN;
            END;

            COMMIT TRANSACTION;
            SELECT CAST(N'replayed' AS nvarchar(16)) AS outcome,
                   @CorteExistente AS corteId,
                   @StatusExistente AS httpStatus,
                   CAST(1 AS bit) AS replayed;
            RETURN;
        END;

        /* Debe compartir esta transacción; DBA valida este supuesto en DEV. */
        INSERT INTO @ResultadoCorte (corte_id)
        EXEC [$(BusinessDatabase)].dbo.SP_CORTE_Crear
            @TipoCorte = @TipoCorte,
            @FechaHoraCorte = @FechaHoraCorte,
            @FechaHoraEjecucion = @FechaHoraEjecucion;

        IF (SELECT COUNT_BIG(*) FROM @ResultadoCorte) <> 1
            THROW 51066, 'SP_CORTE_Crear debe devolver exactamente un corte_id.', 1;

        SELECT @CorteNuevo = corte_id
        FROM @ResultadoCorte;

        INSERT INTO api.IdempotenciaCrearCorte
            (IdempotencyKey, PayloadFingerprint, CorteId, HttpStatus, CreadoUtc, ExpiraUtc)
        VALUES
            (@IdempotencyKey, @PayloadFingerprint, @CorteNuevo, 201, @AhoraUtc,
             DATEADD(HOUR, @VigenciaHoras, @AhoraUtc));

        COMMIT TRANSACTION;

        SELECT CAST(N'created' AS nvarchar(16)) AS outcome,
               @CorteNuevo AS corteId,
               CAST(201 AS smallint) AS httpStatus,
               CAST(0 AS bit) AS replayed;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Solo para el job DBA de retención; no conceder este permiso al login de la API. */
CREATE OR ALTER PROCEDURE api.SP_PurgarIdempotenciaCrearCorte
    @MaxFilas int = 1000
AS
BEGIN
    SET NOCOUNT ON;

    IF @MaxFilas NOT BETWEEN 1 AND 10000
        THROW 51065, 'MaxFilas debe estar entre 1 y 10000.', 1;

    DELETE TOP (@MaxFilas)
    FROM api.IdempotenciaCrearCorte
    WHERE ExpiraUtc <= SYSUTCDATETIME();
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.database_principals
    WHERE name = N'$(ApiLoginName)'
)
BEGIN
    DECLARE @CrearUsuario nvarchar(max) =
        N'CREATE USER [' + REPLACE(N'$(ApiLoginName)', N']', N']]') +
        N'] FOR LOGIN [' + REPLACE(N'$(ApiLoginName)', N']', N']]') + N'];';
    EXEC sys.sp_executesql @CrearUsuario;
END;
GO

DECLARE @OtorgarExec nvarchar(max) =
    N'GRANT EXECUTE ON OBJECT::api.SP_Corte_Crear_Idempotente TO [' +
    REPLACE(N'$(ApiLoginName)', N']', N']]') + N'];';
EXEC sys.sp_executesql @OtorgarExec;
GO

/*
  Rollback (usar solo si ningún despliegue de API depende aún del procedimiento):

  USE [$(IdempotencyDatabase)];
  DROP PROCEDURE IF EXISTS api.SP_PurgarIdempotenciaCrearCorte;
  DROP PROCEDURE IF EXISTS api.SP_Corte_Crear_Idempotente;
  DROP TABLE IF EXISTS api.IdempotenciaCrearCorte;

  El schema api se conserva: podría tener otros objetos del servicio.
*/

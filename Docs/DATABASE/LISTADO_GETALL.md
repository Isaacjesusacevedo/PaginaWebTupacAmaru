# Reescritura de `sp_Listado_GetAll`

## ⚠️ Dos Versiones en Scripts SQL

| Script | Línea | Versión |
|--------|-------|---------|
| `sp_stored_procedures.sql` | 546-566 | **Antigua** (calcula Edad, sin joins académicos) |
| `04_InfAcademica.sql` | 272-293 | **Nueva** (con joins académicos, sin Edad) |

**La que queda en la BD:** La **NUEVA** (de `04_InfAcademica.sql`), porque se ejecuta **después** y hace `CREATE OR ALTER`.

---

## Versión Antigua (en `sp_stored_procedures.sql`)

```sql
CREATE OR ALTER PROCEDURE sp_Listado_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        a.Id AS AlumnoId,
        a.Nombre + ' ' + a.Apellido AS NombreCompleto,
        a.DNI,
        a.Email,
        c.Nombre AS Carrera,
        a.Turno,
        CASE 
            WHEN a.FechaNacimiento IS NOT NULL 
            THEN FLOOR(DATEDIFF(DAY, a.FechaNacimiento, GETDATE()) / 365.25)
            ELSE NULL 
        END AS Edad
    FROM Alumnos a
    LEFT JOIN Carreras c ON a.CarreraId = c.Id
    ORDER BY a.Id;
END;
```

**Características:**
- 1 fila por alumno
- Calcula `Edad` en SQL
- **NO incluye** información académica
- Join simple Alumnos + Carreras

---

## Versión Nueva (en `04_InfAcademica.sql` — **LA QUE QUEDA EN BD**)

```sql
CREATE OR ALTER PROCEDURE sp_Listado_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        a.Id AS AlumnoId,
        a.Nombre + ' ' + a.Apellido AS NombreCompleto,
        a.DNI,
        a.Email,
        c.Nombre AS Carrera,
        a.Turno,
        a.FechaNacimiento,
        ia.Inf_Aca_Descripcion AS TipoAcademico,
        iae.Titulo_Secundario AS TituloSecundario,
        iae.Fecha_Emision AS FechaEmision,
        iae.Estado_Titulo AS EstadoTitulo
    FROM dbo.Alumnos a
    LEFT JOIN dbo.Carreras c ON a.CarreraId = c.Id
    LEFT JOIN dbo.Inf_Academica_Est iae ON iae.ID_Est = a.Id
    LEFT JOIN dbo.Inf_Academica ia ON ia.ID_Inf_Aca = iae.ID_Inf_Aca
    ORDER BY a.Id;
END;
```

**Características:**
- **N filas por alumno** (una por cada registro académico, o 1 fila con NULLs si no tiene ninguno)
- **NO calcula Edad** (se calcula en BR `ListadoService.CalcularEdad`)
- **SÍ incluye** información académica via joins
- Joins: Alumnos → Carreras → Inf_Academica_Est → Inf_Academica
- `ORDER BY a.Id` para agrupar por alumno

---

## Columnas Devueltas (Versión Nueva = Actual en BD)

| Columna | Tipo | Origen | Nullable |
|---------|------|--------|----------|
| `AlumnoId` | int | Alumnos.Id | NO |
| `NombreCompleto` | nvarchar | Alumnos.Nombre + ' ' + Alumnos.Apellido | NO |
| `DNI` | int | Alumnos.DNI | NO |
| `Email` | nvarchar | Alumnos.Email | NO |
| `Carrera` | nvarchar | Carreras.Nombre | SÍ (LEFT JOIN) |
| `Turno` | nvarchar | Alumnos.Turno | SÍ |
| `FechaNacimiento` | datetime | Alumnos.FechaNacimiento | NO |
| `TipoAcademico` | nvarchar | Inf_Academica.Inf_Aca_Descripcion | SÍ (LEFT JOIN) |
| `TituloSecundario` | nvarchar | Inf_Academica_Est.Titulo_Secundario | SÍ |
| `FechaEmision` | date | Inf_Academica_Est.Fecha_Emision | SÍ |
| `EstadoTitulo` | varchar | Inf_Academica_Est.Estado_Titulo | SÍ |

---

## Mapeo a Modelo C# (`ListadoItem`)

```csharp
namespace Instituto.AD.Models;

public class ListadoItem
{
    public int AlumnoId { get; set; }
    public string? NombreCompleto { get; set; }
    public int DNI { get; set; }
    public string? Email { get; set; }
    public string? Carrera { get; set; }
    public string? Turno { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string? TipoAcademico { get; set; }
    public string? TituloSecundario { get; set; }
    public DateTime? FechaEmision { get; set; }
    public string? EstadoTitulo { get; set; }
}
```

**Nota:** `ListadoItem` **no tiene** propiedad `Edad` (se calcula en BR).

---

## Procesamiento en Business Rules (`ListadoService`)

```csharp
public async Task<List<AlumnoListadoDto>> GetListadoAsync()
{
    var items = await _repository.GetListadoAsync();  // Llama SP → List<ListadoItem>

    return items
        .GroupBy(i => i.AlumnoId)                    // Agrupa por alumno
        .Select(ElegirInformacionAcademica)          // Elige 1 registro por alumno
        .OrderBy(dto => dto.AlumnoId)
        .ToList();
}

private static AlumnoListadoDto ElegirInformacionAcademica(IGrouping<int, ListadoItem> grupo)
{
    var elegido = grupo
        .OrderBy(i => PrioridadTipoAcademico(i.TipoAcademico))  // Prioridad por tipo
        .ThenByDescending(i => i.FechaEmision)                  // Más reciente primero
        .First();

    return new AlumnoListadoDto
    {
        AlumnoId = elegido.AlumnoId,
        NombreCompleto = elegido.NombreCompleto,
        DNI = elegido.DNI,
        Email = elegido.Email,
        Carrera = elegido.Carrera,
        Turno = elegido.Turno,
        Edad = CalcularEdad(elegido.FechaNacimiento),           // Calculado en C#
        TipoAcademico = elegido.TipoAcademico,
        EstadoTitulo = elegido.EstadoTitulo,
        FechaEmision = elegido.FechaEmision,
        TituloSecundario = elegido.TipoAcademico == InfAcademicaConstants.TipoTitulo 
            ? elegido.TituloSecundario : null
    };
}

private static int PrioridadTipoAcademico(string? tipo) => tipo switch
{
    InfAcademicaConstants.TipoTitulo => 1,                           // "Título"
    InfAcademicaConstants.TipoTituloEnTramite => 2,                  // "Título en trámite"
    InfAcademicaConstants.TipoConstanciaMateriasAdeudadas => 3,      // "Constancia de materias adeudadas"
    InfAcademicaConstants.TipoConstanciaAlumnoRegular => 3,          // "Constancia de alumno regular"
    _ => 4
};

private static int CalcularEdad(DateTime fechaNacimiento)
{
    var hoy = DateTime.Today;
    var edad = hoy.Year - fechaNacimiento.Year;
    if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
    return edad;
}
```

**Lógica de selección:**
1. Agrupa todos los registros por `AlumnoId`
2. Ordena por **prioridad del tipo académico** (Título > Título en trámite > Constancias)
3. Desempata por **fecha de emisión descendente** (más reciente primero)
4. Toma el **primero** (`First()`)
5. Calcula `Edad` en C# (no en SQL)
6. Solo muestra `TituloSecundario` si el tipo es "Título"

---

## DTO de Salida (`AlumnoListadoDto`)

```csharp
namespace Instituto.BR.DTOs;

public class AlumnoListadoDto
{
    public int AlumnoId { get; set; }
    public string? NombreCompleto { get; set; }
    public int DNI { get; set; }
    public string? Email { get; set; }
    public string? Carrera { get; set; }
    public string? Turno { get; set; }
    public int Edad { get; set; }                    // Calculado en BR
    public string? TipoAcademico { get; set; }
    public string? EstadoTitulo { get; set; }
    public DateTime? FechaEmision { get; set; }
    public string? TituloSecundario { get; set; }    // Solo para tipo "Título"
}
```

---

## Diferencias Clave Resumidas

| Aspecto | Versión Antigua | Versión Nueva (Actual en BD) |
|---------|-----------------|------------------------------|
| Filas por alumno | 1 | N (según registros académicos) |
| Edad | Calculada en SQL (`Edad`) | Calculada en BR (`CalcularEdad`) |
| Info académica | No incluida | Incluida via 2 LEFT JOINs |
| Prioridad | N/A | Resuelta en BR (`PrioridadTipoAcademico`) |
| Columnas devueltas | 7 | 11 |
| Modelo C# mapeado | ❌ No existía | `ListadoItem` → `AlumnoListadoDto` |

---

## Ejecutar la SP

```sql
EXEC sp_Listado_GetAll;
```

**Resultado esperado (ejemplo):**
```
AlumnoId | NombreCompleto | DNI | Email | Carrera | Turno | FechaNacimiento | TipoAcademico | TituloSecundario | FechaEmision | EstadoTitulo
---------|----------------|-----|-------|---------|-------|-----------------|---------------|------------------|--------------|-------------
1        | Ana Martínez   | 40123456 | ... | Desarrollo... | Noche | 2000-05-15 | Título | Bachiller en Cs. | 2023-12-01 | MANO
1        | Ana Martínez   | 40123456 | ... | Desarrollo... | Noche | 2000-05-15 | Constancia de alumno regular | NULL | 2024-03-15 | MANO
2        | Pedro López    | 41234567 | ... | Desarrollo... | Noche | 1999-08-20 | NULL | NULL | NULL | NULL
```
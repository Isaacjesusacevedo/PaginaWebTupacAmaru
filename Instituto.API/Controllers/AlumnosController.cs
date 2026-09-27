using Instituto.BR.Interfaces;
using Instituto.BR.DTOs;
using Instituto.AD.Models;
using Instituto.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Instituto.API.Controllers;

[ApiController]
[Route("api/alumnos")]
public class AlumnosController : ControllerBase
{
    private readonly IAlumnoService _service;

    public AlumnosController(IAlumnoService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize]
    public IActionResult GetAll()
    {
        return Ok(ApiResponse<List<Alumno>>.Success(_service.GetAll()));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public IActionResult GetById(int id)
    {
        var alumno = _service.GetById(id);
        if (alumno is null)
            return NotFound(ApiResponse<Alumno>.Error($"Alumno con Id {id} no fue encontrado."));

        return Ok(ApiResponse<Alumno>.Success(alumno));
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult Create([FromBody] Alumno alumno)
    {
        var result = _service.Create(alumno);
        if (!result.Success)
            return BadRequest(ApiResponse<Alumno>.Error(result.Message!));

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, ApiResponse<Alumno>.Success(result.Data));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public IActionResult Update(int id, [FromBody] Alumno alumno)
    {
        var result = _service.Update(id, alumno);
        if (!result.Success)
            return NotFound(ApiResponse<Alumno>.Error(result.Message!));

        return Ok(ApiResponse<Alumno>.Success(result.Data));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public IActionResult Delete(int id)
    {
        var result = _service.Delete(id);
        if (!result.Success)
            return NotFound(ApiResponse<string>.Error(result.Message!));

        return NoContent();
    }
}
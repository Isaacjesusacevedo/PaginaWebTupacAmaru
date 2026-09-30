using Instituto.BR.Interfaces;
using Instituto.BR.DTOs;
using Instituto.AD.Models;
using Instituto.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Instituto.API.Controllers;

[ApiController]

[Route("api/profesores")]
public class ProfesorController : ControllerBase
{
    private readonly IProfesorService _service;

    public ProfesorController(IProfesorService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(ApiResponse<List<Profesor>>.Success(_service.GetAll()));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var profesor = _service.GetById(id);
        if (profesor is null)
            return NotFound(ApiResponse<Profesor>.Error($"Profesor con Id {id} no fue encontrado."));

        return Ok(ApiResponse<Profesor>.Success(profesor));
    }

    [HttpPost]
    public IActionResult Create([FromBody] Profesor profesor)
    {
        var result = _service.Create(profesor);
        if (!result.Success)
            return BadRequest(ApiResponse<Profesor>.Error(result.Message!));

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, ApiResponse<Profesor>.Success(result.Data));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Profesor profesor)
    {
        var result = _service.Update(id, profesor);
        if (!result.Success)
            return NotFound(ApiResponse<Profesor>.Error(result.Message!));

        return Ok(ApiResponse<Profesor>.Success(result.Data));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var result = _service.Delete(id);
        if (!result.Success)
            return NotFound(ApiResponse<string>.Error(result.Message!));

        return NoContent();
    }
}
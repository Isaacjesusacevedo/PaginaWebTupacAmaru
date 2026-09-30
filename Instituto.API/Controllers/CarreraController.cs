using Instituto.BR.Interfaces;
using Instituto.BR.DTOs;
using Instituto.AD.Models;
using Instituto.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Instituto.API.Controllers;

[ApiController]

[Route("api/carreras")]
public class CarreraController : ControllerBase
{
    private readonly ICarreraService _service;

    public CarreraController(ICarreraService service)
    {
        _service = service;
    }

    [HttpGet]
    
    public IActionResult GetAll()
    {
        return Ok(ApiResponse<List<Carrera>>.Success(_service.GetAll()));
    }

    [HttpGet("{id:int}")]
    
    public IActionResult GetById(int id)
    {
        var carrera = _service.GetById(id);
        if (carrera is null)
            return NotFound(ApiResponse<Carrera>.Error($"Carrera con Id {id} no fue encontrada."));

        return Ok(ApiResponse<Carrera>.Success(carrera));
    }

    [HttpPost]
    public IActionResult Create([FromBody] Carrera carrera)
    {
        var result = _service.Create(carrera);
        if (!result.Success)
            return BadRequest(ApiResponse<Carrera>.Error(result.Message!));

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, ApiResponse<Carrera>.Success(result.Data));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Carrera carrera)
    {
        var result = _service.Update(id, carrera);
        if (!result.Success)
            return NotFound(ApiResponse<Carrera>.Error(result.Message!));

        return Ok(ApiResponse<Carrera>.Success(result.Data));
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
using Instituto.BR.Interfaces;
using Instituto.BR.DTOs;
using Instituto.AD.Models;
using Instituto.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Instituto.API.Controllers;

[ApiController]

[Route("api/formularios")]
public class FormularioController : ControllerBase
{
    private readonly IFormularioService _service;

    public FormularioController(IFormularioService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(ApiResponse<List<Formulario>>.Success(_service.GetAll()));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var formulario = _service.GetById(id);
        if (formulario is null)
            return NotFound(ApiResponse<Formulario>.Error($"Formulario con Id {id} no fue encontrado."));

        return Ok(ApiResponse<Formulario>.Success(formulario));
    }

    [HttpPost]
    public IActionResult Create([FromBody] Formulario formulario)
    {
        var result = _service.Create(formulario);
        if (!result.Success)
            return BadRequest(ApiResponse<Formulario>.Error(result.Message!));

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, ApiResponse<Formulario>.Success(result.Data));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Formulario formulario)
    {
        var result = _service.Update(id, formulario);
        if (!result.Success)
            return NotFound(ApiResponse<Formulario>.Error(result.Message!));

        return Ok(ApiResponse<Formulario>.Success(result.Data));
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
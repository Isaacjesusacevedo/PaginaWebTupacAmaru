using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Route("api/administradores")]
public class AdministradorController : ControllerBase
{
    private readonly ICrudJsonService<Administrador> _service;

    public AdministradorController()
    {
        _service = new CrudJsonService<Administrador>("Data/Administrador.json");
    }

    // 🔹 GET ALL
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAll());
    }

    // 🔹 GET BY ID
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var admin = _service.GetById(id);

        if (admin == null)
            return NotFound("Administrador no encontrado");

        return Ok(admin);
    }

    // 🔹 CREATE
    [HttpPost]
    public IActionResult Create([FromBody] Administrador administrador)
    {
        var list = _service.GetAll();
        administrador.Id = list.Count == 0 ? 1 : list.Max(a => a.Id) + 1;

        _service.Create(administrador);
        return Ok(administrador);
    }

    // 🔹 UPDATE (EDITAR)
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Administrador administrador)
    {
        var existente = _service.GetById(id);

        if (existente == null)
            return NotFound("Administrador no encontrado");

        administrador.Id = id; // aseguramos consistencia
        _service.Update(id, administrador);

        return Ok(administrador);
    }

    // 🔹 DELETE (ELIMINAR)
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var existente = _service.GetById(id);

        if (existente == null)
            return NotFound("Administrador no encontrado");

        _service.Delete(id);
        return NoContent();
    }
}

using Instituto.BR.Interfaces;
using Instituto.BR.DTOs;
using Instituto.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Instituto.API.Controllers;

[ApiController]

[Route("api/listado")]
public class ListadoController : ControllerBase
{
    private readonly IListadoService _service;

    public ListadoController(IListadoService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetListado()
    {
        return Ok(ApiResponse<List<AlumnoListadoDto>>.Success(_service.GetListado()));
    }
}
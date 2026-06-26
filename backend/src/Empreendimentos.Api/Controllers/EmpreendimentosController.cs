using Empreendimentos.Application.DTOs;
using Empreendimentos.Application.Interfaces;
using Empreendimentos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Empreendimentos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmpreendimentosController : ControllerBase
{
    private readonly IEmpreendimentoService _service;

    public EmpreendimentosController(IEmpreendimentoService service)
    {
        _service = service;
    }

    
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<EmpreendimentoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nome,
        [FromQuery] StatusEmpreendimento? status,
        [FromQuery] string ordenarPor = "nome",
        [FromQuery] bool decrescente = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        CancellationToken ct = default)
    {
        var filtro = new FiltroEmpreendimentoDto
        {
            Nome = nome,
            Status = status,
            OrdenarPor = ordenarPor,
            Decrescente = decrescente,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        };

        var resultado = await _service.ListarAsync(filtro, ct);
        return Ok(resultado);
    }

   
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmpreendimentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var resultado = await _service.ObterPorIdAsync(id, ct);
        return Ok(resultado);
    }

    
    [HttpPost]
    [ProducesResponseType(typeof(EmpreendimentoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar([FromBody] CriarEmpreendimentoDto dto, CancellationToken ct)
    {
        var resultado = await _service.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

   
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmpreendimentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarEmpreendimentoDto dto, CancellationToken ct)
    {
        var resultado = await _service.AtualizarAsync(id, dto, ct);
        return Ok(resultado);
    }

    
    [HttpPatch("{id:guid}/inativar")]
    [ProducesResponseType(typeof(EmpreendimentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken ct)
    {
        var resultado = await _service.InativarAsync(id, ct);
        return Ok(resultado);
    }
}
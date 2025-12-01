using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeedBackend_V1.Data;
using SeedBackend_V1.DTOs;
using SeedBackend_V1.Entities;
using static SeedBackend_V1.DTOs.SolicitacaoDtos;


namespace SeedBackend_V1.Controllers;

[ApiController]
[Route("api/v1/solicitacoes")]
public class SolicitacoesController : Controller
{
    private readonly AppDbContext _db;
    public SolicitacoesController(AppDbContext db) => _db = db;

    // GET /solicitacoes?compId=&setorId=&aprovado=
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? compId, [FromQuery] int? setorId, [FromQuery] bool? aprovado)
    {
        var q = _db.SolicitacaoCompra.AsQueryable();

        if (compId.HasValue) q = q.Where(s => s.CompId == compId);
        if (aprovado.HasValue) q = q.Where(s => s.IsAprovado == aprovado);

        // setorId não está direto na tabela; filtragem por setor pode ser feita via relação com usuário requisitor (se necessário)
        var data = await q
            .OrderByDescending(s => s.SolicitacaoId)
            .ToListAsync();

        return Ok(data);
    }

    // POST /solicitacoes
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SolicitacaoCreateDto dto)
    {
        var s = new SolicitacaoCompra
        {
            CompId = dto.CompId,
            Justificativa = dto.Justificativa,
            ValorEstimado = dto.ValorEstimado,
            RequisitorId = dto.RequisitorId,
            DataSolicit = DateTime.UtcNow
        };
        _db.SolicitacaoCompra.Add(s);
        await _db.SaveChangesAsync();

        return Created($"/api/v1/solicitacoes/{s.SolicitacaoId}", new { s.SolicitacaoId });
    }

    // PATCH /solicitacoes/{id}/analise
    [HttpPatch("{id:int}/analise")]
    public async Task<IActionResult> Analisar([FromRoute] int id, [FromBody] SolicitacaoAnaliseDto dto)
    {
        var s = await _db.SolicitacaoCompra.FindAsync(id);
        if (s == null) return NotFound();

        s.IsAprovado = dto.IsAprovado;
        s.AnalistaId = dto.AnalistaId;
        s.DataAnalise = dto.DataAnalise ?? DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Solicitação analisada." });
    }
}

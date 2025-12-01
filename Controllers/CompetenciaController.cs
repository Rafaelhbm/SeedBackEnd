using Microsoft.AspNetCore.Mvc;
using SeedBackend_V1.Data;
using SeedBackend_V1.DTOs;
using SeedBackend_V1.Entities;
using static SeedBackend_V1.DTOs.CompetenciaDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SeedBackend_V1.Controllers;

[ApiController]
[Route("api/v1/competencias")]
[Authorize]
public class CompetenciaController : Controller
{
    private readonly AppDbContext _db;

    public CompetenciaController(AppDbContext db) => _db = db;

    // GET /competencias
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? ano, [FromQuery] int? mes)
    {
        var q = _db.Competencia.AsQueryable();
        if (ano.HasValue) q = q.Where(c => c.Ano == ano.Value);
        if (mes.HasValue) q = q.Where(c => c.Mes == mes.Value);

        var data = await q.Select(c => new CompetenciaResponse(
            c.CompId, c.Ano, c.Mes, c.AbertaPor, c.FechadaPor, c.DataAbertura, c.DataFechamento,
            c.DataFim, 
            c.DataLimite
        )).ToListAsync();

        return Ok(data);
    }

    // POST /competencias
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Abrir([FromBody] CompetenciaCreateDto dto)
    {
        bool exists = await _db.Competencia.AnyAsync(c => c.Ano == dto.Ano && c.Mes == dto.Mes);
        if (exists) return Conflict(new {error = "Competência para esse Ano e Mês já existe."});

        var comp = new Competencia
        {
            Ano = dto.Ano,
            Mes = dto.Mes,
            AbertaPor = dto.AbertaPor,
            DataAbertura = dto.DataAbertura ?? DateTime.UtcNow.Date,
            DataFim = DateTime.SpecifyKind(dto.DataFim, DateTimeKind.Utc),
            DataLimite = DateTime.SpecifyKind(dto.DataLimite, DateTimeKind.Utc)
        };

        _db.Competencia.Add(comp);
        await _db.SaveChangesAsync();
        
        var response = new CompetenciaResponse(
            comp.CompId, comp.Ano, comp.Mes, comp.AbertaPor, comp.FechadaPor, 
            comp.DataAbertura, comp.DataFechamento, comp.DataFim, comp.DataLimite
        );

        return CreatedAtAction(nameof(List), new { ano = comp.Ano, mes = comp.Mes }, response);
    }

    // POST /competencias/{id}/fechar
    [HttpPost("{id:int}/fechar")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Fechar([FromRoute] int id, [FromBody] CompetenciaFecharDto dto)
    {
        var c = await _db.Competencia.FindAsync(id);
        if (c == null) return NotFound();

        c.FechadaPor = dto.FechadaPor;
        c.DataFechamento = dto.DataFechamento ?? DateTime.UtcNow.Date;

        if (c.DataAbertura.HasValue && c.DataFechamento.HasValue && c.DataFechamento < c.DataAbertura)
            return BadRequest(new { error = "Data de fechamento não pode ser anterior à abertura." });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Competência fechada." });
    }
}
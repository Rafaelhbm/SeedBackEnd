using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeedBackend_V1.Data;
using SeedBackend_V1.DTOs;
using SeedBackend_V1.Entities;
using static SeedBackend_V1.DTOs.SetorDtos;
using Microsoft.AspNetCore.Authorization;

namespace SeedBackend_V1.Controllers;

[ApiController]
[Route("api/v1/setores")]
[Authorize]
public class SetoresController : ControllerBase
{
    private readonly AppDbContext _db;
    public SetoresController(AppDbContext db) => _db = db;

    // GET /setores?tipo=IE&paiId=1&q=escola
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? tipo, [FromQuery] int? paiId, [FromQuery] string? q)
    {
        var query = _db.Setor.AsQueryable()
            .Join(_db.TipoSetor, s => s.TipoSetorId, t => t.TipoSetorId,
                  (s, t) => new { s, t });

        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(x => x.t.Codigo == tipo);

        if (paiId.HasValue)
            query = query.Where(x => x.s.SetorPaiId == paiId.Value);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => EF.Functions.ILike(x.s.Nome, $"%{q}%"));

        var data = await query
            .Select(x => new SetorResponse(x.s.SetorId, x.s.Nome, x.t.Codigo, x.s.SetorPaiId))
            .ToListAsync();

        return Ok(data);
    }

    // GET /setores/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var s = await _db.Setor
            .Where(s => s.SetorId == id)
            .Join(_db.TipoSetor, s => s.TipoSetorId, t => t.TipoSetorId, (s, t) => new { s, t })
            .FirstOrDefaultAsync();

        if (s == null) return NotFound();

        return Ok(new SetorResponse(s.s.SetorId, s.s.Nome, s.t.Codigo, s.s.SetorPaiId));
    }

    // POST /setores
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] SetorCreateDTO dto)
    {
        // valida tipo
        var tipo = await _db.TipoSetor.SingleOrDefaultAsync(t => t.Codigo == dto.TipoCodigo);
        if (tipo == null) return BadRequest(new { error = "tipo_codigo inválido" });

        var entity = new Setor
        {
            Nome = dto.Nome,
            TipoSetorId = tipo.TipoSetorId,
            SetorPaiId = dto.SetorPaiId
        };

        _db.Setor.Add(entity);
        await _db.SaveChangesAsync();

        var resp = new SetorResponse(entity.SetorId, entity.Nome, tipo.Codigo, entity.SetorPaiId);
        return CreatedAtAction(nameof(GetById), new { id = entity.SetorId }, resp);
    }

    // ADICIONADO: Endpoint de Update (Editar)
    // PUT /api/v1/setores/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SetorCreateDTO dto)
    {
        // 1. Encontra o setor no banco
        var entity = await _db.Setor.FindAsync(id);
        if (entity == null) return NotFound();

        // 2. Valida o novo Tipo (se ele mudou)
        var tipo = await _db.TipoSetor.SingleOrDefaultAsync(t => t.Codigo == dto.TipoCodigo);
        if (tipo == null) return BadRequest(new { error = "tipo_codigo inválido" });

        // 3. Atualiza os dados
        entity.Nome = dto.Nome;
        entity.TipoSetorId = tipo.TipoSetorId;
        entity.SetorPaiId = dto.SetorPaiId;

        // 4. Salva no banco
        await _db.SaveChangesAsync();
        
        // 5. Retorna o objeto atualizado (usa o 'tipo.Codigo' para o nome 'Tipo' no DTO)
        return Ok(new SetorResponse(entity.SetorId, entity.Nome, tipo.Codigo, entity.SetorPaiId));
    }

    // ADICIONADO: Endpoint de Delete (Excluir)
    // DELETE /api/v1/setores/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        // 1. Encontra o setor no banco
        var entity = await _db.Setor.FindAsync(id);
        if (entity == null) return NotFound();

        // 2. Remove do banco
        _db.Setor.Remove(entity);
        
        // 3. Salva a mudança
        await _db.SaveChangesAsync();
        return NoContent(); // Responde com "sucesso, sem conteúdo"
    }
}
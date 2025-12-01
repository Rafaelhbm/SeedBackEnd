using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeedBackend_V1.Data;
using SeedBackend_V1.Entities;
using SeedBackend_V1.DTOs;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using static SeedBackend_V1.DTOs.ComboDtos;

namespace SeedBackend_V1.Controllers;

[ApiController]
[Route("api/v1/combos")]
[Authorize]
public class CombosController : ControllerBase
{
    private readonly AppDbContext _db;
    public CombosController(AppDbContext db) => _db = db;

    // GET /combos
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? setorId, [FromQuery] int? competenciaId)
    {
        var q = _db.Combo.AsQueryable();
        if (setorId.HasValue) q = q.Where(c => c.SetorId == setorId);
        
        // ADICIONADO FILTRO POR COMPETENCIA
        if (competenciaId.HasValue) q = q.Where(c => c.CompetenciaId == competenciaId);

        var data = await q.Select(c => new {
            c.ComboId,
            c.SetorId,
            c.Descricao,
            c.CompetenciaId
            // Datas e Valor removidos
        }).ToListAsync();

        return Ok(data);
    }

    // GET /combos/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var combo = await _db.Combo.FirstOrDefaultAsync(c => c.ComboId == id);
        if (combo == null) return NotFound();

        var itens = await _db.ItensCombo
            .Where(ic => ic.ComboId == id)
            .Select(ic => new ComboItemDto(ic.ItemId, ic.Quantidade))
            .ToListAsync();

        var resp = new ComboResponse(
            combo.ComboId, combo.SetorId, combo.Descricao,
            combo.CompetenciaId, itens
        );

        return Ok(resp);
    }

    // POST /combos
    [HttpPost]
    [Authorize(Roles = "ADMIN, GESTOR_SEED, DIRETOR_IE")] 
    public async Task<IActionResult> Create([FromBody] ComboCreateDto dto)
    {
        using var tx = await _db.Database.BeginTransactionAsync();

        var combo = new Combo
        {
            SetorId = dto.SetorId,
            Descricao = dto.Descricao,
            CompetenciaId = dto.CompetenciaId
            // Valor e Datas removidos
        };
        
        _db.Combo.Add(combo);
        await _db.SaveChangesAsync();

        // Adiciona itens
        foreach (var it in dto.Itens)
        {
            bool itemExiste = await _db.Item.AnyAsync(i => i.ItemId == it.ItemId);
            if (!itemExiste) return BadRequest(new { error = $"item_id inválido: {it.ItemId}" });

            _db.ItensCombo.Add(new ItensCombo
            {
                ComboId = combo.ComboId,
                ItemId = it.ItemId,
                Quantidade = it.Quantidade
            });
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return CreatedAtAction(nameof(GetById), new { id = combo.ComboId }, new { combo.ComboId });
    }

    // PUT /combos/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN, GESTOR_SEED")] 
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ComboUpdateDto dto)
    {
        using var tx = await _db.Database.BeginTransactionAsync();
        
        var combo = await _db.Combo.FindAsync(id);
        if (combo == null) return NotFound();

        // 1. Atualiza os dados do Combo
        combo.Descricao = dto.Descricao;
        
        _db.Combo.Update(combo);
        
        // 2. Apaga TODOS os ItensCombo antigos
        var oldItens = _db.ItensCombo.Where(ic => ic.ComboId == id);
        _db.ItensCombo.RemoveRange(oldItens);
        
        // 3. Adiciona os Novos ItensCombo (vindos do DTO)
        foreach (var it in dto.Itens)
        {
            bool itemExiste = await _db.Item.AnyAsync(i => i.ItemId == it.ItemId);
            if (!itemExiste) return BadRequest(new { error = $"item_id inválido: {it.ItemId}" });

            _db.ItensCombo.Add(new ItensCombo
            {
                ComboId = combo.ComboId,
                ItemId = it.ItemId,
                Quantidade = it.Quantidade
            });
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { message = "Combo atualizado com sucesso." });
    }

// DELETE /combos/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN, GESTOR_SEED")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var combo = await _db.Combo.FindAsync(id);
        if (combo == null) return NotFound();

        bool gastoUsaCombo = await _db.Gastos.AnyAsync(g => g.ComboId == id);
        if (gastoUsaCombo)
        {
            return BadRequest(new { error = "Este combo não pode ser deletado, pois está associado a um 'Gasto' já existente." });
        }
        
        var itensComboIds = await _db.ItensCombo
            .Where(ic => ic.ComboId == id)
            .Select(ic => ic.ItensComboId)
            .ToListAsync();

        if (itensComboIds.Any())
        {
            bool folhaUsaItem = await _db.FolhaPagamento.AnyAsync(f => 
                f.ItemComboId.HasValue && itensComboIds.Contains(f.ItemComboId.Value)
            );

            if (folhaUsaItem)
            {
                return BadRequest(new { error = "Este combo não pode ser deletado, pois um de seus itens está associado a uma 'Folha de Pagamento' antiga." });
            }
        }
        
        try
        {
            if (itensComboIds.Any())
            {
                var itens = await _db.ItensCombo.Where(ic => itensComboIds.Contains(ic.ItensComboId)).ToListAsync();
                _db.ItensCombo.RemoveRange(itens);
                await _db.SaveChangesAsync();
            }

            _db.Combo.Remove(combo);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { error = "Erro do banco de dados. O combo não pode ser deletado: " + ex.InnerException?.Message ?? ex.Message });
        }
        
        return NoContent(); 
    }
}
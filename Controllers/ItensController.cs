using Microsoft.AspNetCore.Mvc;
using SeedBackend_V1.Data;
using SeedBackend_V1.DTOs;
using SeedBackend_V1.Entities;
using Microsoft.EntityFrameworkCore;
using static SeedBackend_V1.DTOs.ItemDtos;
using Microsoft.AspNetCore.Authorization;

namespace SeedBackend_V1.Controllers;

[ApiController]
[Route("api/v1/itens")]
[Authorize]
public class ItensController : Controller
{
    private readonly AppDbContext _db;
    public ItensController(AppDbContext db) => _db = db;

    // GET /itens?q=papel por exemplo
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? q)
    {
        var query = _db.Item.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(query => EF.Functions.ILike(query.Nome, $"%{q}%"));

        var data = await query
            .Select(i => new ItemResponse(i.ItemId, i.Nome, i.Descricao, i.UnidadeDeMedida))
            .ToListAsync();
        return Ok(data);
    }

    // POST /itens
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] ItemCreateDto dto)
    {
        var it = new Item 
        { 
            Nome = dto.Nome, 
            Descricao = dto.Descricao,
            UnidadeDeMedida = dto.UnidadeDeMedida
        };
        
        _db.Item.Add(it);
        await _db.SaveChangesAsync();

        var resp = new ItemResponse(it.ItemId, it.Nome, it.Descricao, it.UnidadeDeMedida);
        return CreatedAtAction(nameof(List), new { q = it.Nome }, resp);
    }

    // PUT /api/v1/itens/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ItemCreateDto dto)
    {
        // 1. Encontra o item no banco
        var item = await _db.Item.FindAsync(id);
        if (item == null) return NotFound(new { error = "Item não encontrado." });

        // 2. Atualiza os dados
        item.Nome = dto.Nome;
        item.Descricao = dto.Descricao;
        item.UnidadeDeMedida = dto.UnidadeDeMedida;

        // 3. Salva no banco
        await _db.SaveChangesAsync();
        
        // 4. Retorna o objeto atualizado
        var resp = new ItemResponse(item.ItemId, item.Nome, item.Descricao, item.UnidadeDeMedida);
        return Ok(resp);
    }


    // DELETE /itens/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var item = await _db.Item.FindAsync(id);
        if (item == null) return NotFound(new { error = "Item não encontrado." });

        _db.Item.Remove(item);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
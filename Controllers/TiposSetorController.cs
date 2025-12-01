using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeedBackend_V1.Data;

namespace SeedBackend_V1.Controllers;

// Vai listar os tipos de setor que existem
[ApiController]
[Route("api/v1/tipos-setor")]
public class TiposSetorController : Controller
{
    private readonly AppDbContext _db;
    public TiposSetorController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var data = await _db.TipoSetor
            .Where(t => t.IsAtivo)
            .Select(t => new { t.Codigo, t.Nome })
            .ToListAsync();
        return Ok(data);
    }
}

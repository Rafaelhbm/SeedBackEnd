using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeedBackend_V1.Data;
using SeedBackend_V1.DTOs;
using SeedBackend_V1.Entities;
using System;
using System.Threading.Tasks;
using static SeedBackend_V1.DTOs.GastoDtos;
using Microsoft.AspNetCore.Authorization; 

namespace SeedBackend_V1.Controllers
{
    [ApiController]
    [Route("api/v1/gastos")]
    [Authorize] 
    public class GastosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public GastosController(AppDbContext db)
        {
            _db = db;
        }

        /// RF006: Permite que RH (e outros) informem/criem gastos.
        /// RF008: Apenas se a competência estiver aberta.
        [HttpPost]
        [Authorize(Roles = "RH, GESTOR_SEED, ADMIN, DIRETOR_IE, USUARIO")] // ADICIONEI DIRETOR_IE/USUARIO pois escola lança gasto
        public async Task<IActionResult> CreateGasto([FromBody] GastoCreateDto dto)
        {
            // RF008: Verificar se a competência está aberta
            var competencia = await _db.Competencia.FindAsync(dto.CompId);
            if (competencia == null)
                return BadRequest(new { error = "Competência não encontrada." });

            if (competencia.DataFechamento != null)
                return BadRequest(new { error = "Não é possível adicionar gastos. A competência está fechada." }); // RF008

            var setorExiste = await _db.Setor.AnyAsync(s => s.SetorId == dto.SetorId);
            if (!setorExiste)
                return BadRequest(new { error = "Setor não encontrado." });
            
            var gasto = new Gasto
            {
                CompId = dto.CompId,
                SetorId = dto.SetorId,
                ComboId = dto.ComboId,
                SolicitCompra = dto.SolicitacaoId, 
                AprovadorId = dto.AprovadorId,
                DataCadastro = DateTime.UtcNow,

                // NOVOS CAMPOS: Salvando item por item
                ItemId = dto.ItemId,
                Quantidade = dto.Quantidade,
                Valor = dto.Valor
            };

            _db.Gastos.Add(gasto);
            await _db.SaveChangesAsync(); 

            return CreatedAtAction(nameof(CreateGasto), new { id = gasto.GastoId }, gasto);
        }

        /// Permite a listagem de gastos para os perfis autorizados.
        [HttpGet]
        [Authorize(Roles = "RH, GESTOR_SEED, ADMIN, DIRETOR_IE, USUARIO, ANALISTA_COMPRAS")] // ADICIONEI ESCOLA PARA PODER VER O QUE LANÇOU
        public async Task<IActionResult> ListGastos([FromQuery] int? compId, [FromQuery] int? setorId)
        {
            var query = _db.Gastos.AsQueryable();

            if (compId.HasValue)
                query = query.Where(g => g.CompId == compId.Value);

            if (setorId.HasValue)
                query = query.Where(g => g.SetorId == setorId.Value);

            var gastos = await query
                .OrderByDescending(g => g.DataCadastro)
                .ToListAsync();

            return Ok(gastos);
        }

        /// RF005: Permite que GESTOR/ADMIN gerenciem (editem) gastos.
        /// RF008: Apenas se a competência (original e nova) estiver aberta.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "GESTOR_SEED, ADMIN")] // RF005, RF004
        public async Task<IActionResult> UpdateGasto(int id, [FromBody] GastoCreateDto dto)
        {
            var gasto = await _db.Gastos.FindAsync(id);
            if (gasto == null)
                return NotFound();

            // RF008: Verifica a competência original do gasto
            var competenciaOriginal = await _db.Competencia.FindAsync(gasto.CompId);
            if (competenciaOriginal?.DataFechamento != null)
                return BadRequest(new { error = "Não é possível editar gastos. A competência original está fechada." });

            // RF008: Se o gasto for movido para outra competência, verifica a nova
            if (gasto.CompId != dto.CompId)
            {
                var competenciaNova = await _db.Competencia.FindAsync(dto.CompId);
                if (competenciaNova == null) 
                    return BadRequest(new { error = "Nova competência não encontrada." });
                if (competenciaNova.DataFechamento != null) 
                    return BadRequest(new { error = "Não é possível mover o gasto. A nova competência está fechada." });
            }

            var setorExiste = await _db.Setor.AnyAsync(s => s.SetorId == dto.SetorId);
            if (!setorExiste) 
                return BadRequest(new { error = "Setor não encontrado." });

            // Mapeia o DTO para a entidade existente
            gasto.CompId = dto.CompId;
            gasto.SetorId = dto.SetorId;
            
            gasto.ComboId = dto.ComboId;
            gasto.SolicitCompra = dto.SolicitacaoId; //
            gasto.AprovadorId = dto.AprovadorId;
            
            // NOVOS CAMPOS NA EDIÇÃO TAMBÉM
            gasto.ItemId = dto.ItemId;
            gasto.Quantidade = dto.Quantidade;
            gasto.Valor = dto.Valor;
            
            await _db.SaveChangesAsync(); 
            return Ok(gasto);
        }

        /// RF005: Permite que GESTOR/ADMIN gerenciem (excluam) gastos.
        /// RF008: Apenas se a competência estiver aberta.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "GESTOR_SEED, ADMIN")] // RF005, RF004
        public async Task<IActionResult> DeleteGasto(int id)
        {
            var gasto = await _db.Gastos.FindAsync(id);
            if (gasto == null)
                return NotFound();

            // RF008: Verifica a competência do gasto
            var competencia = await _db.Competencia.FindAsync(gasto.CompId);
            if (competencia?.DataFechamento != null)
                return BadRequest(new { error = "Não é possível excluir gastos. A competência está fechada." });

            _db.Gastos.Remove(gasto);
            await _db.SaveChangesAsync(); 
            
            return NoContent(); // Sucesso, sem conteúdo
        }
    }
}
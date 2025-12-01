using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeedBackend_V1.Data;
using static SeedBackend_V1.DTOs.AlunosDtos;
using SeedBackend_V1.Entities;
using Npgsql;

namespace SeedBackend_V1.Controllers
{
    [ApiController]
    [Route("api/v1/alunos")]
    public class AlunosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AlunosController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlunos([FromQuery] int compId, [FromQuery] int setorId)
        {
            // Busca se existe registro para essa escola (setorId) nessa competência (compId)
            var registros = await _db.AlunosPorSetorCompetencia
                .Where(a => a.CompId == compId && a.SetorId == setorId)
                .ToListAsync();

            // Retorna a lista (O front pega o primeiro item da lista)
            return Ok(registros);
        }

        [HttpGet("contagem-por-ie")]
        public async Task<IActionResult> GetContagemAlunosPorIE()
        {
            var contagem = await _db.Setor
                .Join(_db.TipoSetor, s => s.TipoSetorId, ts => ts.TipoSetorId, (s, ts) => new { Setor = s, TipoSetor = ts })
                .Where(x => x.TipoSetor.Codigo == "IE")
                .GroupJoin(_db.AlunosPorSetorCompetencia,
                           setorInfo => setorInfo.Setor.SetorId,
                           alunos => alunos.SetorId,
                           (setorInfo, alunos) => new { setorInfo.Setor, Alunos = alunos })
                .Select(result => new ContagemAlunosPorIEResponse(
                    result.Setor.SetorId,
                    result.Setor.Nome,
                    result.Alunos.Sum(a => (long)a.Quantidade)
                ))
                .ToListAsync();

            return Ok(contagem);
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarAlunos([FromBody] AlunosUpsertDto dto)
        {
            try
            {
                // Tenta encontrar um registro existente com a chave primária composta
                var registro = await _db.AlunosPorSetorCompetencia
                    .FindAsync(dto.SetorId, dto.CompId);

                if (registro != null)
                {
                    // Atualiza o existente
                    registro.Quantidade = dto.Quantidade;
                    registro.UsuarioRegistro = dto.UsuarioRegistro;
                    _db.AlunosPorSetorCompetencia.Update(registro);
                }
                else
                {
                    // Cria um novo
                    var novoRegistro = new AlunosSetorComp
                    {
                        SetorId = dto.SetorId,
                        CompId = dto.CompId,
                        Quantidade = dto.Quantidade,
                        UsuarioRegistro = dto.UsuarioRegistro
                    };
                    _db.AlunosPorSetorCompetencia.Add(novoRegistro);
                }

                await _db.SaveChangesAsync();
                return Ok(new { message = "Registro de alunos salvo com sucesso." });
            }
            catch (DbUpdateException ex)
            {
                // Verifica se a exceção interna é uma violação de constraint do PostgreSQL
                if (ex.InnerException is PostgresException pgEx)
                {

                    if (pgEx.Message.Contains("apenas setores do tipo IE podem registrar alunos"))
                    {
                        return BadRequest(new { error = pgEx.Message });
                    }
                }
                // Se for outra exceção de banco, retorna 500 (ou deixa o handler global pegar)
                throw;
            }
        }
    }
}
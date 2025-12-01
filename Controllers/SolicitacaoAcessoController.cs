using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SeedBackend_V1.Data;
using SeedBackend_V1.DTOs;
using SeedBackend_V1.Entities;
using SeedBackend_V1.Services;
using static SeedBackend_V1.DTOs.SolicitacaoAcessoDtos;

namespace SeedBackend_V1.Controllers
{
    [ApiController]
    [Route("api/v1/solicitacoes-acesso")]
    public class SolicitacaoAcessoController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;

        public SolicitacaoAcessoController(AppDbContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] SolicitacaoAcessoCreateDto dto)
        {
            var solicitacao = new SolicitacaoAcesso
            {
                NomeCompleto = dto.NomeCompleto,
                Email = dto.Email,
                Justificativa = dto.Justificativa,
                PerfilDesejadoId = dto.PerfilDesejadoId,
                DataSolicitacao = DateTime.UtcNow
            };
            _db.SolicitacaoAcesso.Add(solicitacao);
            await _db.SaveChangesAsync();

            try
            {
                // Enviar email para o administrador
                var adminEmail = _db.Usuario.FirstOrDefault(u => u.Perfil.TipoPerfil == "ADMIN")?.Email;
                if (adminEmail is not null)
                {
                    var subjectAdmin = "Nova Solicitação de Acesso Recebida";
                    var bodyAdmin = $"Uma nova solicitação de acesso foi feita por {dto.NomeCompleto} ({dto.Email}).<br/>Por favor, analise no painel administrativo.";
                    await _emailService.SendEmailAsync(adminEmail, subjectAdmin, bodyAdmin);
                }


                // Enviar email de confirmação para o requisitante
                var subjectUser = "Sua Solicitação de Acesso foi Recebida";
                var bodyUser = $"Olá {dto.NomeCompleto},<br/><br/>Recebemos a sua solicitação de acesso ao sistema. A sua requisição está a ser analisada e entraremos em contato em breve.<br/><br/>Atenciosamente,<br/>Equipe SEED";
                await _emailService.SendEmailAsync(dto.Email, subjectUser, bodyUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email de notificação: {ex.Message}");
            }

            return StatusCode(201, new { id = solicitacao.SolicitacaoAcessoId });
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")] 
        public async Task<IActionResult> List([FromQuery] string? status)
        {
            var query = _db.SolicitacaoAcesso.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.Status == status.ToUpper());
            }

            var requests = await query
                .OrderByDescending(s => s.DataSolicitacao)
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPatch("{id:int}/analise")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Analyse([FromRoute] int id, [FromBody] SolicitacaoAcessoAnaliseDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var solicitacao = await _db.SolicitacaoAcesso.FindAsync(id);
                if (solicitacao == null) return NotFound();

                solicitacao.Status = dto.Status.ToUpper();
                solicitacao.AnalisadoPorId = dto.AnalisadoPorId;
                solicitacao.DataAnalise = DateTime.UtcNow;

                if (solicitacao.Status == "APROVADO")
                {
                    bool emailExists = await _db.Usuario.AnyAsync(u => u.Email == solicitacao.Email);
                    if (emailExists)
                    {
                        await transaction.RollbackAsync();
                        return Conflict(new { error = "Um usuário com este email já existe." });
                    }

                    string tempPassword = Guid.NewGuid().ToString();

                    const string sql = @"
                INSERT INTO usuario (nome, email, perfil_id, setor_id, senha_hash, dt_cadastro)
                VALUES (@nome, @email, @perfilId, @setorId, crypt(@senha, gen_salt('bf', 10)), CURRENT_DATE);";

                    await _db.Database.ExecuteSqlRawAsync(sql,
                        new NpgsqlParameter("@nome", solicitacao.NomeCompleto),
                        new NpgsqlParameter("@email", solicitacao.Email),
                        new NpgsqlParameter("@perfilId", solicitacao.PerfilDesejadoId),
                        new NpgsqlParameter("@setorId", 1), // Setor padrão
                        new NpgsqlParameter("@senha", tempPassword)
                    );
                }

                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { message = $"Solicitação {id} foi atualizada para {solicitacao.Status}." });
            }
            catch (Exception)
            {
                // Se qualquer erro ocorrer, desfaz todas as alterações.
                await transaction.RollbackAsync();
                return StatusCode(500, "Ocorreu um erro interno ao processar a solicitação.");
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql; 
using SeedBackend_V1.Data;
using SeedBackend_V1.Entities;
using SeedBackend_V1.DTOs; 
using Microsoft.AspNetCore.Authorization;

namespace SeedBackend_V1.Controllers
{
    [ApiController]
    [Route("api/v1/usuarios")]
    [Authorize(Roles = "ADMIN")] // Protege todos os métodos deste controller
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsuarioController(AppDbContext db)
        {
            _db = db;
        }

        // MÉTODO GET (LISTAR)
        [HttpGet]
        public async Task<IActionResult> List()
        {
            // Busca todos os usuários e inclui seus perfis
            var usuarios = await _db.Usuario
                .Include(u => u.Perfil) // Inclui a entidade Perfil
                .OrderBy(u => u.Nome)
                .Select(u => new UsuarioDtos.UsuarioDto // Usa o DTO para formatar a resposta
                {
                    UserId = u.UserId,
                    Nome = u.Nome,
                    Email = u.Email,
                    PerfilId = u.PerfilId,
                    PerfilNome = u.Perfil.TipoPerfil, // Pega o nome do perfil
                    SetorId = u.SetorId,
                    DtCadastro = u.DtCadastro
                })
                .ToListAsync();
            
            return Ok(usuarios);
        }

        // MÉTODO POST (CRIAR)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UsuarioDtos.UsuarioCreateDto dto)
        {
            const string sql = @"
                INSERT INTO usuario (nome, perfil_id, setor_id, email, senha_hash, dt_cadastro)
                VALUES (@nome, @perfilId, @setorId, @email, crypt(@senha, gen_salt('bf', 10)), CURRENT_DATE)
                RETURNING user_id;";

            await using var conn = (NpgsqlConnection)_db.Database.GetDbConnection();
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", dto.Nome);
            cmd.Parameters.AddWithValue("@perfilId", dto.PerfilId);
            cmd.Parameters.AddWithValue("@setorId", (object?)dto.SetorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email", dto.Email);
            cmd.Parameters.AddWithValue("@senha", dto.Senha);

            var newUserId = await cmd.ExecuteScalarAsync();

            return CreatedAtAction(nameof(Create), new { id = newUserId }, new { userId = newUserId });
        }
        
        // MÉTODO PUT (ATUALIZAR)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UsuarioDtos.UsuarioUpdateDto dto)
        {
            // 1. Busca e rastreia a entidade
            var usuario = await _db.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            // 2. Atualiza apenas os campos modificáveis
            usuario.Nome = dto.Nome;
            usuario.Email = dto.Email;
            
            // Lógica para resetar a senha (se uma nova senha foi enviada)
            if (dto.PerfilId > 0)
            {
                usuario.PerfilId = dto.PerfilId;
            }

            if (dto.SetorId > 0) 
            {
                usuario.SetorId = dto.SetorId;
            }
            
            // 3. Atualiza Senha via SQL direto se necessário
            if (!string.IsNullOrWhiteSpace(dto.Senha))
            {
                // Este comando SQL atualiza a senha usando crypt
                const string sql = "UPDATE usuario SET senha_hash = crypt(@senha, gen_salt('bf', 10)) WHERE user_id = @id";
                await _db.Database.ExecuteSqlRawAsync(sql,
                    new NpgsqlParameter("@senha", dto.Senha),
                    new NpgsqlParameter("@id", id)
                );
            }

            // Se a data estiver como Unspecified, forçamos UTC para o Npgsql não reclamar
            if (usuario.DtCadastro.Kind == DateTimeKind.Unspecified)
            {
                usuario.DtCadastro = DateTime.SpecifyKind(usuario.DtCadastro, DateTimeKind.Utc);
            }

            try 
            {
                // REMOVIDO: _db.Usuario.Update(usuario); 
                // Motivo: O Update() marca TODOS os campos como modificados, forçando o envio da Data.
                // Como já usamos FindAsync, o EF Core já sabe o que mudou.
                
                await _db.SaveChangesAsync();

                // Recarrega o perfil para retorno
                var perfilDesc = "Desconhecido";
                var perfil = await _db.Perfil.FindAsync(usuario.PerfilId);
                if (perfil != null) perfilDesc = perfil.TipoPerfil;

                var usuarioDto = new UsuarioDtos.UsuarioDto
                {
                    UserId = usuario.UserId,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    PerfilId = usuario.PerfilId,
                    SetorId = usuario.SetorId,
                    DtCadastro = usuario.DtCadastro,
                    PerfilNome = perfilDesc
                };
                
                return Ok(usuarioDto);
            }
            catch (DbUpdateException ex)
            {
                // Captura erro de banco (ex: FK inexistente, email duplicado)
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest($"Erro no banco de dados: {message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        // MÉTODO DELETE (DELETAR)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var usuario = await _db.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _db.Usuario.Remove(usuario);
            await _db.SaveChangesAsync();

            return NoContent(); // Sucesso, sem conteúdo para retornar
        }
    }
}
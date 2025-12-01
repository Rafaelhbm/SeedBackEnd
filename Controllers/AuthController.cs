using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using SeedBackend_V1.Data;
using SeedBackend_V1.DTOs;
using LoginRequest = SeedBackend_V1.DTOs.LoginRequest;

namespace SeedBackend_V1.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;
    
    public AuthController(AppDbContext db, IConfiguration cfg)
    { _db = db; _cfg = cfg; }
    
    // POST /auth/login
    [AllowAnonymous]
    [HttpPost("login")] // Mudar para botar o /api/v1/login depois (sem tempo irmão) - Fazer funcionar primeiro é mais interessante
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password)) {return BadRequest("E-Mail e senha são necessários.");}

        await using var conn = (NpgsqlConnection)_db.Database.GetDbConnection();
        await conn.OpenAsync();
        
        // Aqui é nossa validação (meio óbvio)
        // Adicionei u.setor_id aqui no SELECT
        const string sql = @"
            SELECT u.user_id, u.nome, p.tipo_perfil, u.setor_id
            FROM usuario u
            JOIN perfil p ON p.perfil_id = u.perfil_id
            WHERE u.email = @e AND u.senha_hash = crypt(@p, u.senha_hash);"; //Depois eu vou montar uma classe só para os comandos de SQL. 
                                                                            // Eu poderia usar o EF, mas isso é mais seguro (acho)
        await using var cmd = new NpgsqlCommand(sql,conn);
        cmd.Parameters.AddWithValue("@e", req.Email);
        cmd.Parameters.AddWithValue("@p", req.Password);
        
        await using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync())
            return Unauthorized("Credenciais inválidas.");

        var userId = rd.GetInt32(0); // <-- Mudança: capturar o userId
        var nome = rd.GetString(1);
        var perfil = rd.GetString(2);

        // Ler o setor_id (verificando se é nulo no banco)
        // O índice é 3 porque adicionamos ele como a quarta coluna no SELECT
        int? setorId = rd.IsDBNull(3) ? null : rd.GetInt32(3);
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        // Mudei para List<Claim> para facilitar adicionar o setor se ele existir
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, req.Email),
            new Claim(ClaimTypes.Name, nome),
            new Claim(ClaimTypes.Role, perfil),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", userId.ToString()) // <-- Mudança: adicionar ID ao token
        };

        // Se tiver setor, adiciona no token também
        if (setorId.HasValue)
        {
            claims.Add(new Claim("setorId", setorId.Value.ToString()));
        }
        
        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        
        return Ok(new {
            token = jwt,
            name = nome,
            perfil = perfil,
            userId = userId, // O ID que o frontend precisa
            setorId = setorId // O frontend precisa disso para não dar Erro Crítico
        });
    }
}
using System.ComponentModel.DataAnnotations;

namespace SeedBackend_V1.DTOs
{
    // Este "static class" permite que os controllers
    // chamem os DTOs como "UsuarioDtos.UsuarioDto"
    public static class UsuarioDtos
    {
        // DTO para CRIAR um usuário
        public class UsuarioCreateDto
        {
            [Required] public string Nome { get; set; } = string.Empty;
            [Required] public int PerfilId { get; set; }
            [Required] public int SetorId { get; set; }
            [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
            [Required] [MinLength(6)] public string Senha { get; set; } = string.Empty;
        }

        // DTO para ATUALIZAR um usuário
        public class UsuarioUpdateDto
        {
            [Required] public string Nome { get; set; } = string.Empty;
            [Required] public int PerfilId { get; set; }
            [Required] public int SetorId { get; set; }
            [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
            
            // Senha é opcional na atualização
            public string? Senha { get; set; } 
        }

        // DTO para LISTAR usuários
        public class UsuarioDto
        {
            public int UserId { get; set; }
            public string Nome { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public int PerfilId { get; set; }
            public string PerfilNome { get; set; } = string.Empty; // Nome do Perfil
            public int SetorId { get; set; }
            public DateTime DtCadastro { get; set; }
        }
    }
}
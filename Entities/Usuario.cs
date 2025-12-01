namespace SeedBackend_V1.Entities
{
    public class Usuario
    {
        public int UserId { get; set; }
        public string Nome { get; set; } = null!;
        public int PerfilId { get; set; }
        public DateTime DtCadastro { get; set; }
        public int SetorId { get; set; }
        public string Email { get; set; } = null!;
        public string SenhaHash { get; set; } = null!;
        public virtual Perfil Perfil { get; set; } = null!;
    }
}

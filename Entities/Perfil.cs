namespace SeedBackend_V1.Entities
{
    public class Perfil
    {
        public int PerfilId { get; set; }
        public string TipoPerfil { get; set; } = null!;
        public bool IsAtivo { get; set; } = true;
    }
}

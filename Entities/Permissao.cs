namespace SeedBackend_V1.Entities
{
    public class Permissao
    {
        public int PermId { get; set; }
        public string Codigo { get; set; } = null!;
        public string PermNome { get; set; } = null!;
        public string? PermDescricao { get; set; }
    }
}

namespace SeedBackend_V1.Entities
{
    public class TipoSetor
    {
        public int TipoSetorId { get; set; }
        public string Codigo { get; set; } = null!; // 'IE', 'DR', 'SEED'...
        public string Nome { get; set; } = null!;
        public string? Descricao { get; set; }
        public bool IsAtivo { get; set; } = true;
    }
}

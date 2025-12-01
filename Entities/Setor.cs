namespace SeedBackend_V1.Entities
{
    public class Setor
    {
        public int SetorId { get; set; }
        public string Nome { get; set; } = null!;
        public int TipoSetorId { get; set; }
        public int? RepresentanteId { get; set; }
        public int? SetorPaiId { get; set; }
    }
}

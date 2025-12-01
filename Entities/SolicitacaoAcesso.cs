namespace SeedBackend_V1.Entities
{
    public class SolicitacaoAcesso
    {
        public int SolicitacaoAcessoId { get; set; }
        public string NomeCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Justificativa { get; set; }
        public int PerfilDesejadoId { get; set; }
        public string Status { get; set; } = "PENDENTE";
        public DateTime DataSolicitacao { get; set; }
        public DateTime? DataAnalise { get; set; }
        public int? AnalisadoPorId { get; set; }
    }
}
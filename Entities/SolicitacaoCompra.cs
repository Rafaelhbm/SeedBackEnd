using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeedBackend_V1.Entities
{
    [Table("solicitacao_compra")] // "Etiqueta" que diz o nome real da tabela
    public class SolicitacaoCompra
    {
        [Key] // define a chave primária
        public int SolicitacaoId { get; set; }
        public string? Justificativa { get; set; }
        public int CompId { get; set; }
        public DateTime DataSolicit { get; set; }
        public DateTime? DataAnalise { get; set; }
        public bool? IsAprovado { get; set; }
        public decimal? ValorEstimado { get; set; }
        public int RequisitorId { get; set; }
        public int? AnalistaId { get; set; }
    }
}
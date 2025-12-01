using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeedBackend_V1.Entities
{
    [Table("setor_historico")] // <-- ETIQUETA
    public class SetorHistorico
    {
        [Key] // <-- CHAVE
        public long HistoricoId { get; set; }
        public int SetorId { get; set; }
        public string? Nome { get; set; }
        public string? TipoSetor { get; set; }
        public string? Endereco { get; set; }
        public int? RepresentanteId { get; set; }
        public DateTime DataAlteracao { get; set; }
    }
}
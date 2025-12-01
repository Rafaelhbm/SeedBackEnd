using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeedBackend_V1.Entities
{
    [Table("competencia")] // <-- ETIQUETA
    public class Competencia
    {
        [Key] // <-- CHAVE
        public int CompId { get; set; }
        public int Ano { get; set; }
        public int Mes { get; set; }
        public DateTime? DataAbertura { get; set; }
        public DateTime? DataFechamento { get; set; }
        public DateTime? DataFim { get; set; }
        public DateTime? DataLimite { get; set; }
        public int AbertaPor { get; set; }
        public int? FechadaPor { get; set; }
    }
}
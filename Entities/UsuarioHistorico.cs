using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeedBackend_V1.Entities
{
    [Table("usuario_historico")] // <-- ETIQUETA
    public class UsuarioHistorico
    {
        [Key] // <-- CHAVE
        public long UserHistId { get; set; }
        public int UserId { get; set; }
        public string? Nome { get; set; }
        public string? TipoPerfil { get; set; }
        public DateTime? Cadastro { get; set; }
        public int? SetorId { get; set; }
        public DateTime DataAlteracao { get; set; }
    }
}
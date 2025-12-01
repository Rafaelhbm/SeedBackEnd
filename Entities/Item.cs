using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeedBackend_V1.Entities
{
    [Table("item")] // "Etiqueta" que diz o nome real da tabela no SQL
    public class Item
    {
        [Key] // define a chave primária
        public int ItemId { get; set; }
        public string Nome { get; set; } = null!;
        public string? Descricao { get; set; }
        public string UnidadeDeMedida { get; set; } = null!;
    }
}
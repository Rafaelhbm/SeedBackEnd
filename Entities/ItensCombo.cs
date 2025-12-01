using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeedBackend_V1.Entities
{
    [Table("itens_combo")] // "Etiqueta" que diz o nome real da tabela no SQL
    public class ItensCombo
    {
        [Key] // define a chave primária
        public int ItensComboId { get; set; }
        public int ComboId { get; set; }
        public int ItemId { get; set; }
        public int Quantidade { get; set; }
    }
}
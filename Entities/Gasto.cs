using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeedBackend_V1.Entities
{
    [Table("gastos")] // <-- ETIQUETA
    public class Gasto
    {
        [Key] // <-- CHAVE
        public int GastoId { get; set; }
        
        // RELAÇÃO COM O PACOTE (COMBO) E APROVAÇÃO
        public int? ComboId { get; set; }
        public int CompId { get; set; }
        public int SetorId { get; set; }
        public int? AprovadorId { get; set; }
        public int? SolicitCompra { get; set; }
        public DateTime DataCadastro { get; set; }

        // NOVOS CAMPOS (PARA SALVAR O DETALHE DO ITEM)
        public int ItemId { get; set; }       // Qual item foi comprado?
        public int Quantidade { get; set; }   // Quantos foram comprados?
        public decimal Valor { get; set; }    // Valor total DESTE item
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeedBackend_V1.Entities
{
    [Table("folha_pagamento")] // <-- ETIQUETA
    public class FolhaPagamento
    {
        [Key] // <-- CHAVE
        public int FolhaId { get; set; }
        public int UsuarioRegistro { get; set; }
        public int SetorId { get; set; }
        public int CompId { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataRegistro { get; set; }
        public int? ItemComboId { get; set; }
    }
}
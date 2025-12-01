namespace SeedBackend_V1.Entities
{
    public class Combo
    {
        public int ComboId { get; set; }
        public string? Descricao { get; set; }
        // public DateTime? ValidadeInicio { get; set; } // <-- REMOVIDO
        // public DateTime? ValidadeFim { get; set; } // <-- REMOVIDO
        // public decimal ValorCombo { get; set; } // <-- REMOVIDO
        public int SetorId { get; set; }
        public int? CompetenciaId { get; set; } // <-- ADICIONADO
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeedBackend_V1.Entities
{
    [Table("alunos_por_setor_competencia")] // <-- ETIQUETA
    public class AlunosSetorComp
    {
        // Esta entidade tem uma chave composta (SetorId, CompId)
        // que já está definida no AppDbContext.cs, então não precisamos
        // de um [Key] aqui. Apenas a etiqueta [Table] é necessária.
        public int SetorId { get; set; }
        public int CompId { get; set; }
        public int Quantidade { get; set; }
        public int UsuarioRegistro { get; set; }
    }
}
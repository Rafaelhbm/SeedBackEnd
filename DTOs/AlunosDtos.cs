namespace SeedBackend_V1.DTOs
{
    public class AlunosDtos
    {
        public record AlunosUpsertDto(
            int SetorId,
            int CompId,
            int Quantidade,
            int UsuarioRegistro
        );

        public record ContagemAlunosPorIEResponse(
            int SetorId,
            string NomeSetor,
            long TotalAlunos
        );
    }
}

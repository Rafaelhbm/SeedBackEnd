namespace SeedBackend_V1.DTOs
{
    public class CompetenciaDtos
    {
        public record CompetenciaCreateDto(
            int Ano,
            int Mes,
            int AbertaPor,
            DateTime? DataAbertura,
            DateTime DataFim,
            DateTime DataLimite
        );

        public record CompetenciaFecharDto(
            int FechadaPor, 
            DateTime? DataFechamento);

        public record CompetenciaResponse(
            int compId,
            int Ano,
            int Mes,
            int? AbertaPor, int?FechadaPor,
            DateTime? DataAbertura, DateTime? DataFechamento,
            DateTime? DataFim,
            DateTime? DataLimite
            );
    }
}
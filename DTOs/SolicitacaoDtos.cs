namespace SeedBackend_V1.DTOs
{
    public class SolicitacaoDtos
    {
        public record SolicitacaoCreateDto(
            int CompId,
            string? Justificativa,
            decimal? ValorEstimado,
            int RequisitorId
            );

        public record SolicitacaoAnaliseDto(
            bool IsAprovado,
            int AnalistaId,
            DateTime? DataAnalise
            );
    }
}

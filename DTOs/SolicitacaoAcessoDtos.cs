namespace SeedBackend_V1.DTOs
{
    public class SolicitacaoAcessoDtos
    {
        public record SolicitacaoAcessoCreateDto(
            string NomeCompleto,
            string Email,
            string? Justificativa,
            int PerfilDesejadoId
        );

        // DTO para um admin analisar o pedido
        public record SolicitacaoAcessoAnaliseDto(
            string Status, // "APROVADO" ou "REJEITADO"
            int AnalisadoPorId
        );
    }
}
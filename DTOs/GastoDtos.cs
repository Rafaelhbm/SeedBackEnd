namespace SeedBackend_V1.DTOs
{
    public class GastoDtos
    {
        public record GastoCreateDto(
            int CompId,
            int SetorId,
            int? ComboId,
            int? SolicitacaoId,
            int? AprovadorId,
            
            // MUDANÇA: Agora recebemos os dados do item individual
            int ItemId,
            int Quantidade,
            decimal Valor
        );
    }
}
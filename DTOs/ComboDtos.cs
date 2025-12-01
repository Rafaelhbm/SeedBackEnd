namespace SeedBackend_V1.DTOs
{
    public class ComboDtos
    {
        public record ComboItemDto(
            int ItemId,
            int Quantidade
            );

        public record ComboCreateDto(
            int SetorId,
            string? Descricao,
            // decimal ValorCombo, // <-- REMOVIDO
            // DateTime? ValidadeInicio, // <-- REMOVIDO
            // DateTime? ValidadeFim, // <-- REMOVIDO
            int CompetenciaId, // <-- ADICIONADO
            List<ComboItemDto> Itens
            );

        public record ComboUpdateDto(
            string? Descricao,
            // decimal ValorCombo, // <-- REMOVIDO
            // DateTime? ValidadeInicio, // <-- REMOVIDO
            // DateTime? ValidadeFim, // <-- REMOVIDO
            List<ComboItemDto> Itens 
        );

        public record ComboResponse(
            int ComboId,
            int SetorId,
            string? Descricao,
            // decimal ValorCombo, // <-- REMOVIDO
            // DateTime? ValidadeInicio, // <-- REMOVIDO
            // DateTime? ValidadeFim, // <-- REMOVIDO
            int? CompetenciaId, // <-- ADICIONADO
            List<ComboItemDto> Itens
            );
    }
}
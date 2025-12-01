namespace SeedBackend_V1.DTOs
{
    public class ItemDtos
    {
        public record ItemCreateDto (
            string Nome,
            string? Descricao,
            string UnidadeDeMedida
            );

        public record ItemResponse(
            int ItemId,
            string Nome,
            string? Descricao,
            string UnidadeDeMedida
            );
    }
}

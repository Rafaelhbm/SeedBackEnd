namespace SeedBackend_V1.DTOs
{
    public class FolhaDtos
    {
        public record FolhaCreateDto(
            int UsuarioRegistro,
            int SetorId,
            int CompId,
            decimal ValorTotal,
            int? ItemComboId
        );
    }
}

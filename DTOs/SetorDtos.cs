namespace SeedBackend_V1.DTOs
{
    public class SetorDtos
    {
        public record SetorCreateDTO(
            string Nome,
            string TipoCodigo,
            int? SetorPaiId
            );

        public record SetorResponse(
            int setorId,
            string Nome,
            string Tipo,
            int? SetorPaiId
            );
    }
}

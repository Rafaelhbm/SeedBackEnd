namespace SeedBackend_V1.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Name, string Perfil);
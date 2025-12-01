using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Npgsql.NameTranslation;
using SeedBackend_V1.Data;
using SeedBackend_V1.Services;
using SeedBackend_V1.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. CONEXÃO COM O BANCO (Com verificação de segurança)
var connectionString = builder.Configuration.GetConnectionString("SeedDb") 
    ?? throw new InvalidOperationException("ERRO CRÍTICO: A ConnectionString 'SeedDb' não foi encontrada no appsettings.json.");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt
        .UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention()
);

// EmailService
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// CORS
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") 
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// 2. JWT (Com verificação de segurança)
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("ERRO CRÍTICO: A configuração 'Jwt:Key' não foi encontrada no appsettings.json.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// PIPELINE
app.UseRouting();
app.UseCors(myAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
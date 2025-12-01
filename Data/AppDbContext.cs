using Microsoft.EntityFrameworkCore;
using SeedBackend_V1.Entities;
namespace SeedBackend_V1.Data;

// DbContext é a classe que representa a sessão com o banco de dados e permite consultar e salvar dados nele.
// Pelo EF ( entity framework ) o DbContext é a principal classe que interage com o banco de dados, caro leitor!
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) {}

    // Agora a parte infernal de montar cada DbSet:
    // Perfis
    public DbSet<Perfil> Perfil => Set<Perfil>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<PermissaoPerfil> PermissaoPerfil => Set<PermissaoPerfil>();

    // Setores | Setor sector
    public DbSet<TipoSetor> TipoSetor => Set<TipoSetor>();
    public DbSet<Setor> Setor => Set<Setor>();
    public DbSet<SetorHistorico> SetorHistorico => Set<SetorHistorico>();

    // Usuarios | Users
    public DbSet<Usuario> Usuario => Set<Usuario>();
    public DbSet<UsuarioHistorico> UsuarioHistorico => Set<UsuarioHistorico>();

    // Competencia
    public DbSet<Competencia> Competencia => Set<Competencia>();

    // Itens / Gastos / Combos / Solicitacoes / Parte financeira
    public DbSet<Item> Item => Set<Item>();
    public DbSet<Combo> Combo => Set<Combo>();
    public DbSet<ItensCombo> ItensCombo => Set<ItensCombo>();

    public DbSet<SolicitacaoCompra> SolicitacaoCompra => Set<SolicitacaoCompra>();
    public DbSet<Gasto> Gastos => Set<Gasto>();
    public DbSet<FolhaPagamento> FolhaPagamento => Set<FolhaPagamento>();
    public DbSet<SolicitacaoAcesso> SolicitacaoAcesso => Set<SolicitacaoAcesso>();

    // Alunos por IE
    public DbSet<AlunosSetorComp> AlunosPorSetorCompetencia => Set<AlunosSetorComp>();
    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Mapeamento explícito de todas as chaves primárias
        mb.Entity<Perfil>().HasKey(e => e.PerfilId);
        mb.Entity<Permissao>().HasKey(e => e.PermId);
        mb.Entity<PermissaoPerfil>().HasKey(e => e.PermPerfId);
        mb.Entity<TipoSetor>().HasKey(e => e.TipoSetorId);
        mb.Entity<Setor>().HasKey(e => e.SetorId);
        mb.Entity<SetorHistorico>().HasKey(e => e.HistoricoId);
        mb.Entity<Usuario>().HasKey(e => e.UserId);
        mb.Entity<UsuarioHistorico>().HasKey(e => e.UserHistId);
        mb.Entity<Competencia>().HasKey(e => e.CompId);
        mb.Entity<Item>().HasKey(e => e.ItemId);
        mb.Entity<Combo>().HasKey(e => e.ComboId);
        mb.Entity<ItensCombo>().HasKey(e => e.ItensComboId);
        mb.Entity<SolicitacaoCompra>().HasKey(e => e.SolicitacaoId);
        mb.Entity<Gasto>().HasKey(e => e.GastoId);
        mb.Entity<FolhaPagamento>().HasKey(e => e.FolhaId);
        mb.Entity<SolicitacaoAcesso>().HasKey(e => e.SolicitacaoAcessoId);

        // TABELAS (ToTable é opcional com SnakeCase, mas mantive explícito)
        mb.Entity<Perfil>().ToTable("perfil");
        mb.Entity<Permissao>().ToTable("permissoes");
        mb.Entity<PermissaoPerfil>().ToTable("permissao_perfil");

        mb.Entity<TipoSetor>().ToTable("tipo_setor");
        mb.Entity<Setor>().ToTable("setor");
        mb.Entity<SetorHistorico>().ToTable("setor_historico");

        mb.Entity<Usuario>().ToTable("usuario");
        mb.Entity<UsuarioHistorico>().ToTable("usuario_historico");

        mb.Entity<Competencia>().ToTable("competencia");

        mb.Entity<Item>().ToTable("item");
        mb.Entity<Combo>().ToTable("combo");
        mb.Entity<ItensCombo>().ToTable("itens_combo");

        mb.Entity<SolicitacaoCompra>().ToTable("solicitacao_compra");
        mb.Entity<Gasto>().ToTable("gastos");
        mb.Entity<FolhaPagamento>().ToTable("folha_pagamento");

        mb.Entity<SolicitacaoAcesso>().ToTable("solicitacao_acesso");

        mb.Entity<AlunosSetorComp>().ToTable("alunos_por_setor_competencia");

        // UNIQUE (perfil_id, perm_id) na tabela junção
        mb.Entity<PermissaoPerfil>()
            .HasIndex(x => new { x.PerfilId, x.PermId })
            .IsUnique();

        // UNIQUE (combo_id, item_id) para evitar item repetido no combo
        mb.Entity<ItensCombo>()
            .HasIndex(x => new { x.ComboId, x.ItemId })
            .IsUnique();

        // PK composta em alunos_por_setor_competencia
        mb.Entity<AlunosSetorComp>()
            .HasKey(x => new { x.SetorId, x.CompId });

        // Hierarquia de Setor (pai e filho) com ON DELETE SET NULL
        mb.Entity<Setor>()
            .HasOne<Setor>()
            .WithMany()
            .HasForeignKey(s => s.SetorPaiId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relaciona Setor → TipoSetor
        mb.Entity<Setor>()
            .HasOne<TipoSetor>()
            .WithMany()
            .HasForeignKey(s => s.TipoSetorId);

        // Índices úteis (equivalentes aos do SQL)
        mb.Entity<Usuario>().HasIndex(u => u.SetorId);
        mb.Entity<Usuario>().HasIndex(u => u.PerfilId);
        mb.Entity<Combo>().HasIndex(c => c.SetorId);
        mb.Entity<SolicitacaoCompra>().HasIndex(s => new { s.CompId, s.IsAprovado });
        mb.Entity<Gasto>().HasIndex(g => new { g.CompId, g.SetorId });
        mb.Entity<Gasto>().HasIndex(g => g.SetorId);
        mb.Entity<FolhaPagamento>().HasIndex(f => new { f.SetorId, f.CompId });

        base.OnModelCreating(mb);
    }
}

using Empreendimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Empreendimentos.Infrastructure.Data;

public class AppDbContext : DbContext
{
    // Nota: a entidade Empreendimento usa setters privados e construtor privado
    // de propósito (encapsulamento de invariantes de domínio). O EF Core 8
    // consegue materializá-la normalmente via reflection, sem necessidade de
    // expor setters públicos só para satisfazer o ORM.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Empreendimento> Empreendimentos => Set<Empreendimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empreendimento>(builder =>
        {
            builder.ToTable("empreendimentos");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.Nome)
                .HasColumnName("nome")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Cnpj)
                .HasColumnName("cnpj")
                .HasMaxLength(14)
                .IsFixedLength()
                .IsRequired();

            // Regra: CNPJ deve ser único no banco (constraint física, além da
            // verificação na camada de aplicação, para garantir consistência
            // mesmo em cenários de concorrência).
            builder.HasIndex(e => e.Cnpj)
                .IsUnique()
                .HasDatabaseName("ux_empreendimentos_cnpj");

            builder.Property(e => e.Endereco)
                .HasColumnName("endereco")
                .HasMaxLength(300)
                .IsRequired(false);

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.DataCriacao)
                .HasColumnName("data_criacao")
                .IsRequired();

            builder.Property(e => e.DataAtualizacao)
                .HasColumnName("data_atualizacao")
                .IsRequired(false);
        });
    }
}

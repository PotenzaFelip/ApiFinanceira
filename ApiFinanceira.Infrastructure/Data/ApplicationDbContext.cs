using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Enums;

namespace ApiFinanceira.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Pessoa> Pessoas { get; set; }
        public DbSet<Conta> Contas { get; set; }
        public DbSet<Cartao> Cartoes { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pessoa>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Documento).IsUnique();
                entity.Property(p => p.Nome).IsRequired().HasMaxLength(255);
                entity.Property(p => p.Documento).IsRequired().HasMaxLength(14);
                entity.Property(p => p.SenhaHash).IsRequired();
                entity.Property(p => p.CreatedAt).IsRequired();
                entity.Property(p => p.UpdatedAt).IsRequired();

                entity.HasMany(p => p.Contas)
                      .WithOne(c => c.Pessoa)
                      .HasForeignKey(c => c.PessoaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Conta>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Branch).IsRequired().HasMaxLength(3);
                entity.Property(c => c.Account).IsRequired().HasMaxLength(9);
                entity.HasIndex(c => c.Account).IsUnique();
                entity.Property(c => c.Saldo).HasColumnType("numeric(18,2)");
                entity.Property(c => c.Limite).HasColumnType("numeric(18,2)");
                entity.Property(c => c.CreatedAt).IsRequired();
                entity.Property(c => c.UpdatedAt).IsRequired();

                entity.HasMany(c => c.Cartoes)
                      .WithOne(ca => ca.Conta)
                      .HasForeignKey(ca => ca.ContaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Transacoes)
                      .WithOne()
                      .HasForeignKey(t => t.ContaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Cartao>(entity =>
            {
                entity.Property(c => c.Type).IsRequired().HasMaxLength(10); 
                entity.Property(c => c.Number).IsRequired().HasMaxLength(19);
                entity.Property(c => c.Cvv).IsRequired().HasMaxLength(3);

               
                entity.HasIndex(c => c.Number).IsUnique();

                
                entity.HasOne(c => c.Conta)
                      .WithMany(co => co.Cartoes)
                      .HasForeignKey(c => c.ContaId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade); 
            });


            modelBuilder.Entity<Conta>()
                .Property(c => c.Saldo)
                .HasColumnType("decimal(18,2)");

        }
    }
}

using Microsoft.EntityFrameworkCore;
using System.IO;
using WpfMercearia.Models;

namespace WpfMercearia.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<ItemVenda> ItensVenda { get; set; }

      
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
                {
                    string pastaDados = Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        "WpfMercearia");

                    Directory.CreateDirectory(pastaDados);

                    string caminhoBanco = Path.Combine(
                        pastaDados,
                        "WpfMercearia.db");

                    optionsBuilder.UseSqlite(
                        $"Data Source={caminhoBanco}");
                }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Codigo)
                    .IsRequired(false)
                    .HasMaxLength(50);

                entity.HasIndex(p => p.Codigo)
                    .IsUnique();

                entity.Property(p => p.Nome)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(p => p.Preco)
                    .HasPrecision(10, 2);

                entity.Property(p => p.Unidade)
                    .HasMaxLength(10);
            });

            modelBuilder.Entity<Venda>(entity =>
            {
                entity.HasKey(v => v.Id);

                entity.Property(v => v.Total)
                    .HasPrecision(10, 2);

                entity.Property(v => v.FormaPagamento)
                    .IsRequired()
                    .HasMaxLength(30);
            });

            modelBuilder.Entity<ItemVenda>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.Quantidade)
                    .HasPrecision(10, 3);

                entity.Property(i => i.PrecoUnitario)
                    .HasPrecision(10, 2);

                entity.Ignore(i => i.Subtotal);

                entity.HasOne(i => i.Venda)
                    .WithMany(v => v.Itens)
                    .HasForeignKey(i => i.VendaId);

                entity.HasOne(i => i.Produto)
                    .WithMany(p => p.ItensVenda)
                    .HasForeignKey(i => i.ProdutoId)
                    .OnDelete(DeleteBehavior.Restrict); ;
            });
        }
    }
}


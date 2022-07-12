using Microsoft.EntityFrameworkCore;
using MinimalApiCatalogo.Models;

namespace MinimalApiCatalogo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //Mapeamento das Entidades
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Produto> Produtos { get; set; }

        //OnModel Creating que usa uma instancia Model Builder como parametro
        //Fluent Api usada para configurar classes de dominio e substituir as convencoes do EF Core
        protected override void OnModelCreating(ModelBuilder mb)
        {
            //Configurando a Entidade Categoria
            //Definindo a chave primaria
            mb.Entity<Categoria>().HasKey(c => c.CategoriaId);

            //Definindo tamanho das colunas
            mb.Entity<Categoria>().Property(c => c.Nome).HasMaxLength(100).IsRequired();
            mb.Entity<Categoria>().Property(c => c.Descricao).HasMaxLength(150).IsRequired();

            //Configurando a Entidade Produto
            //Definindo a chave primaria
            mb.Entity<Produto>().HasKey(p => p.ProdutoId);

            //Definindo tamanho das colunas
            mb.Entity<Produto>().Property(p => p.Nome).HasMaxLength(100).IsRequired();
            mb.Entity<Produto>().Property(p => p.Descricao).HasMaxLength(150).IsRequired();
            mb.Entity<Produto>().Property(p => p.Imagem).HasMaxLength(100);
            mb.Entity<Produto>().Property(p => p.Preco).HasPrecision(14, 2);

            //Definindo Relacionamento
            mb.Entity<Produto>().HasOne<Categoria>(c => c.Categoria).WithMany(p => p.Produtos).HasForeignKey(c => c.CategoriaId);
        }
    }
}

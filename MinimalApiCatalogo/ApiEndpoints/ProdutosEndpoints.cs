using Microsoft.EntityFrameworkCore;
using MinimalApiCatalogo.Data;
using MinimalApiCatalogo.Models;

namespace MinimalApiCatalogo.ApiEndpoints
{
    public static class ProdutosEndpoints
    {
        public static void MapProutosEndpoints(this WebApplication app)
        {
            //Definindo os EndPoints de Produtos
            app.MapPost("/produtos", async (Produto produto, AppDbContext db) =>
            {
                db.Produtos.Add(produto);
                await db.SaveChangesAsync();

                return Results.Created($"/produtos/{produto.ProdutoId}", produto);
            }).RequireAuthorization();

            app.MapGet("/produtos", async (AppDbContext db) => await db.Produtos.ToListAsync()).RequireAuthorization();

            app.MapGet("/produtos/{id:int}", async (AppDbContext db, int id) =>
            {
                return await db.Produtos.FindAsync(id)
                                is Produto produto
                                ? Results.Ok(produto)
                                : Results.NotFound("Produto não encontrada");
            }).RequireAuthorization();

            app.MapPut("/produtos/{id:int}", async (int id, Produto produto, AppDbContext db) =>
            {
                if (produto.ProdutoId != id)
                {
                    return Results.BadRequest("Id Incorreto");
                }
                var produtoDb = await db.Produtos.FindAsync(id);
                if (produtoDb is null) return Results.NotFound("Id não encontrado ou não existe");

                produtoDb.Nome = produto.Nome;
                produtoDb.Descricao = produto.Descricao;
                produtoDb.Preco = produto.Preco;
                produtoDb.Imagem = produto.Imagem;
                produtoDb.Estoque = produto.Estoque;
                produtoDb.CategoriaId = produto.CategoriaId;

                await db.SaveChangesAsync();
                return Results.Ok(produtoDb);
            }).RequireAuthorization();

            app.MapDelete("/produtos/{id:int}", async (int id, AppDbContext db) =>
            {
                var produto = await db.Produtos.FindAsync(id);
                if (produto is null)
                {
                    return Results.NotFound("Id não encontrado ou não existe");
                }
                db.Produtos.Remove(produto);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).RequireAuthorization();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using MinimalApiCatalogo.Data;
using MinimalApiCatalogo.Models;

namespace MinimalApiCatalogo.ApiEndpoints
{
    public static class CategoriasEndpoints
    {
        public static void MapCategoriasEndpoints(this WebApplication app)
        {
            //Definindo os EndPoints de Categorias
            app.MapGet("/", () => "Catalogo de Produtos - 2022").ExcludeFromDescription();

            app.MapPost("/categorias", async (AppDbContext db, Categoria categoria) =>
            {
                db.Categorias.Add(categoria);
                await db.SaveChangesAsync();
                return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
            }).RequireAuthorization();

            app.MapGet("/categorias", async (AppDbContext db) => await db.Categorias.ToListAsync()).RequireAuthorization();

            app.MapGet("/categorias/{id:int}", async (AppDbContext db, int id) =>
            {
                return await db.Categorias.FindAsync(id)
                                is Categoria categoria
                                ? Results.Ok(categoria)
                                : Results.NotFound("Categoria não encontrada");
            }).RequireAuthorization();

            app.MapPut("/categorias/{id:int}", async (int id, Categoria categoria, AppDbContext db) =>
            {
                if (categoria.CategoriaId != id)
                {
                    return Results.BadRequest("Id Incorreto");
                }
                var categoriaDb = await db.Categorias.FindAsync(id);
                if (categoriaDb is null) return Results.NotFound("Id não encontrado ou não existe");

                categoriaDb.Nome = categoria.Nome;
                categoriaDb.Descricao = categoria.Descricao;

                await db.SaveChangesAsync();
                return Results.Ok(categoriaDb);
            }).RequireAuthorization();

            app.MapDelete("/categorias/{id:int}", async (int id, AppDbContext db) =>
            {
                var categoria = await db.Categorias.FindAsync(id);
                if (categoria is null)
                {
                    return Results.NotFound("Id não encontrado ou não existe");
                }
                db.Categorias.Remove(categoria);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).RequireAuthorization();
        }
    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApiCatalogo.Data;
using MinimalApiCatalogo.Models;
using MinimalApiCatalogo.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.//ConfigureServices
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MinimalApiCatalogo", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Bearer scheme.
                        Enter 'Bearer'[space].Example: \'Bearer 12345abcdef\'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

//Para o MySql
//builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddSingleton<ITokenService>(new TokenService());
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                                  {
                                      options.TokenValidationParameters = new TokenValidationParameters
                                      {
                                          ValidateIssuer = true,
                                          ValidateAudience = true,
                                          ValidateLifetime = true,
                                          ValidateIssuerSigningKey = true,

                                          ValidIssuer = builder.Configuration["Jwt:Issuer"],
                                          ValidAudience = builder.Configuration["Jwt:Audience"],
                                          IssuerSigningKey = new SymmetricSecurityKey
                                          (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                                      };
                                  });
builder.Services.AddAuthorization();

var app = builder.Build();

//endpoint para login
app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) =>
{
    if(userModel is null)
    {
        return Results.BadRequest("Login Inválido");
    }
    if(userModel.UserName == "FelipeMuniz" && userModel.Password == "numsey#123")
    {
        var tokenString = tokenService.GerarToken(app.Configuration["Jwt:Key"],
                                                  app.Configuration["Jwt:Issuer"],
                                                  app.Configuration["Jwt:Audience"],
                                                  userModel);
        return Results.Ok(new { token = tokenString });
    }
    else
    {
        return Results.BadRequest("Login Inválido");
    }
}).Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status200OK).WithName("Login").WithTags("Autenticacao");

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

app.MapDelete("/categorias/{id:int}", async(int id, AppDbContext db) =>
{
    var categoria = await db.Categorias.FindAsync(id);
    if(categoria is null)
    {
        return Results.NotFound("Id não encontrado ou não existe");
    }
    db.Categorias.Remove(categoria);
    await db.SaveChangesAsync();

    return Results.NoContent();
}).RequireAuthorization();

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

// Configure the HTTP request pipeline.//Configure
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
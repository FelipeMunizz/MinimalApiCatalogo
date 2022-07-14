using MinimalApiCatalogo.ApiEndpoints;
using MinimalApiCatalogo.AppServicesExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiSwagger();
builder.AddPersistence();
builder.AddAutenticationJwt();
builder.Services.AddCors();

var app = builder.Build();


app.MapAutenticacaoEndpoints();
app.MapCategoriasEndpoints();
app.MapProutosEndpoints();

var environment = app.Environment;
app.UseExceptionHandling(environment)
    .UseSwaggerMiddleware()
    .UseAppCors();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
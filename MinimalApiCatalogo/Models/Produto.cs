namespace MinimalApiCatalogo.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public string? Imagem { get; set; }
        public DateTime DataCompra { get; set; }
        public int Estoque { get; set; }
    }
}

namespace MetaPDV.Models
{
    public class Mercadoria
    {
        public int Id { get; set; }
        public string CodigoBarras { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }
        public int QuantidadeMinima { get; set; }
        public decimal Total => Preco * QuantidadeEstoque;

    }
}

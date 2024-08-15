using MetaPDV.Models.Vendas;

namespace MetaPDV.Models
{
    public class Venda
    {
        public int Id { get; set; }
        public List<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
        public decimal Total => Itens.Sum(i => i.Total);
        public DateTime DataVenda { get; set; }
        public bool ShowDetails { get; set; }
    }
}

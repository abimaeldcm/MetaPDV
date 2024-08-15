namespace MetaPDV.Models.Vendas
{
    public class ItemVenda
    {
        public int Id { get; set; }
        public Mercadoria MercadoriaInfo { get; set; }
        public int Quantidade { get; set; }
        public decimal Total { get; set; }
    }
}

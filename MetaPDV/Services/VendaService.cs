using Dapper;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetaPDV.Models;
using MetaPDV.Models.Vendas;

namespace MetaPDV.Services
{
	public class VendaService
	{
		private readonly string _connectionString;

		public VendaService(string connectionString)
		{
			_connectionString = connectionString;
		}

        public async Task AdicionarVenda(Venda venda)
        {
            venda.DataVenda = DateTime.Now;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Inserir a venda
                        var sqlVenda = @"
                        INSERT INTO public.""Vendas"" (""DataVenda"", ""ShowDetails"")
                        VALUES (@DataVenda, @ShowDetails)
                        RETURNING ""Id""";

                        var vendaId = await connection.QuerySingleAsync<int>(
                            sqlVenda,
                            new
                            {
                                venda.DataVenda,
                                venda.ShowDetails
                            },
                            transaction
                        );

                        // Inserir itens da venda
                        var sqlItemVenda = @"
                        INSERT INTO public.""ItensVenda"" (""MercadoriaId"", ""Quantidade"", ""Total"", ""VendaId"")
                        VALUES (@MercadoriaId, @Quantidade, @Total, @VendaId)";

                        foreach (var item in venda.Itens)
                        {
                            await connection.ExecuteAsync(
                                sqlItemVenda,
                                new
                                {
                                    MercadoriaId = item.MercadoriaInfo.Id,
                                    item.Quantidade,
                                    item.Total,
                                    VendaId = vendaId
                                },
                                transaction
                            );

                            // Atualizar o estoque
                            var sqlUpdateEstoque = @"
                            UPDATE public.""Mercadorias""
                            SET ""QuantidadeEstoque"" = ""QuantidadeEstoque"" - @Quantidade
                            WHERE ""Id"" = @MercadoriaId";

                            await connection.ExecuteAsync(
                                sqlUpdateEstoque,
                                new
                                {
                                    Quantidade = item.Quantidade,
                                    MercadoriaId = item.MercadoriaInfo.Id
                                },
                                transaction
                            );
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new ApplicationException("Erro ao adicionar a venda", ex);
                    }
                }
            }
        }


        public async Task<IEnumerable<Venda>> ListarVendas()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var sql = @"
                    SELECT v.*, iv.*, m.*
                    FROM public.""Vendas"" v
                    LEFT JOIN public.""ItensVenda"" iv ON v.""Id"" = iv.""VendaId""
                    LEFT JOIN public.""Mercadorias"" m ON iv.""MercadoriaId"" = m.""Id""
                    ORDER BY v.""Id"", iv.""Id""";

                    var vendas = new Dictionary<int, Venda>();

                    var result = await connection.QueryAsync<Venda, ItemVenda, Mercadoria, Venda>(
                        sql,
                        (venda, itemVenda, mercadoria) =>
                        {
                            if (!vendas.TryGetValue(venda.Id, out var vendaEntry))
                            {
                                vendaEntry = venda;
                                vendaEntry.Itens = new List<ItemVenda>();
                                vendas.Add(vendaEntry.Id, vendaEntry);
                            }

                            if (itemVenda != null)
                            {
                                itemVenda.MercadoriaInfo = mercadoria;
                                vendaEntry.Itens.Add(itemVenda);
                            }

                            return vendaEntry;
                        },
                        splitOn: "Id,Id,Id");

                    return vendas.Values;
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Erro ao listar vendas", ex);
                }
            }
        }
    }
}

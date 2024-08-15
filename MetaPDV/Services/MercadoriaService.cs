using Dapper;
using Npgsql;
using MetaPDV.Models;

namespace MetaPDV.Services
{
    public class MercadoriaService
    {
        private readonly string _connectionString;

        public MercadoriaService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AdicionarMercadoria(Mercadoria mercadoria)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var sql = @"
                    INSERT INTO public.""Mercadorias"" 
                    (""CodigoBarras"", ""Nome"", ""Preco"", ""QuantidadeEstoque"", ""QuantidadeMinima"")
                    VALUES (@CodigoBarras, @Nome, @Preco, @QuantidadeEstoque, @QuantidadeMinima)";

                    await connection.ExecuteAsync(sql, new
                    {
                        mercadoria.CodigoBarras,
                        mercadoria.Nome,
                        mercadoria.Preco,
                        mercadoria.QuantidadeEstoque,
                        mercadoria.QuantidadeMinima
                    });
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Erro ao adicionar mercadoria", ex);
                }
            }
        }

        public async Task AtualizarMercadoria(Mercadoria mercadoria)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var sql = @"
                    UPDATE public.""Mercadorias""
                    SET ""CodigoBarras"" = @CodigoBarras, 
                        ""Nome"" = @Nome, 
                        ""Preco"" = @Preco, 
                        ""QuantidadeEstoque"" = @QuantidadeEstoque, 
                        ""QuantidadeMinima"" = @QuantidadeMinima
                    WHERE ""Id"" = @Id";

                    var rowsAffected = await connection.ExecuteAsync(sql, new
                    {
                        mercadoria.Id,
                        mercadoria.CodigoBarras,
                        mercadoria.Nome,
                        mercadoria.Preco,
                        mercadoria.QuantidadeEstoque,
                        mercadoria.QuantidadeMinima
                    });

                    if (rowsAffected == 0)
                    {
                        throw new KeyNotFoundException("Mercadoria não encontrada");
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Erro ao atualizar mercadoria", ex);
                }
            }
        }

        public async Task ExcluirMercadoria(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var sql = "DELETE FROM public.\"Mercadorias\" WHERE \"Id\" = @Id";

                    var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

                    if (rowsAffected == 0)
                    {
                        throw new KeyNotFoundException("Mercadoria não encontrada");
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Erro ao excluir mercadoria", ex);
                }
            }
        }

        public async Task<IEnumerable<Mercadoria>> ListarMercadorias()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var sql = "SELECT * FROM public.\"Mercadorias\" ORDER BY \"Id\" ASC";
                    return await connection.QueryAsync<Mercadoria>(sql);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Erro ao listar mercadorias", ex);
                }
            }
        }

        public async Task<Mercadoria> BuscarMercadoriaPorId(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var sql = "SELECT * FROM public.\"Mercadorias\" WHERE \"Id\" = @Id";
                    return await connection.QueryFirstOrDefaultAsync<Mercadoria>(sql, new { Id = id });
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Erro ao buscar mercadoria por ID", ex);
                }
            }
        }
    }
}

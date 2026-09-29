using Microsoft.EntityFrameworkCore;
using WpfMercearia.Data;
using WpfMercearia.Models;

namespace WpfMercearia.Services
{
    public class ProdutoService
    {
        public async Task<Produto?> BuscarPorCodigoAsync(string codigo)
        {
            using var context = new AppDbContext();

            return await context.Produtos
                .FirstOrDefaultAsync(p => p.Codigo == codigo);
        }

        public async Task<List<Produto>> BuscarProdutosPorPesoAsync()
        {
            using var context = new AppDbContext();

            return await context.Produtos
                .Where(p => p.VendidoPorPeso)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }
        public async Task<List<Produto>> BuscarProdutosSemCodigoAsync()
        {
            using var context = new AppDbContext();

            return await context.Produtos
                .Where(p =>
                    string.IsNullOrEmpty(p.Codigo) &&
                    !p.VendidoPorPeso)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task CadastrarProdutoAsync(Produto produto)
        {
            using var context = new AppDbContext();

            context.Produtos.Add(produto);

            await context.SaveChangesAsync();
        }

        public async Task<List<Produto>> ListarProdutosAsync()
        {
            using var context = new AppDbContext();

            return await context.Produtos
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task AtualizarProdutoAsync(Produto produto)
        {
            using var context = new AppDbContext();

            context.Produtos.Update(produto);

            await context.SaveChangesAsync();
        }

        public async Task ExcluirProdutoAsync(int id)
        {
            using var context = new AppDbContext();

            var produto = await context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                return;

            var possuiVendas = await context.ItensVenda
                .AnyAsync(i => i.ProdutoId == id);

            if (possuiVendas)
            {
                throw new InvalidOperationException(
                    $"O produto '{produto.Nome}' já possui vendas registradas e não pode ser excluído.");
            }

            context.Produtos.Remove(produto);

            await context.SaveChangesAsync();
        }
    }
}
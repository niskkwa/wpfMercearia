using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using WpfMercearia.Data;
using WpfMercearia.Models;

namespace WpfMercearia.Services
{
    public class VendaService
    {
        public ObservableCollection<ItemCarrinho> Itens { get; } = new();

        public decimal Total =>
            Itens.Sum(item => item.Subtotal);

        public void AdicionarProduto(Produto produto)
        {
            if (produto.VendidoPorPeso)
                throw new InvalidOperationException(
                    "Este produto é vendido por peso.");

            var itemExistente = Itens.FirstOrDefault(
                item => item.Produto.Id == produto.Id);

            if (itemExistente != null)
            {
                itemExistente.Quantidade++;
                return;
            }

            Itens.Add(new ItemCarrinho
            {
                Produto = produto,
                Quantidade = 1,
                PrecoUnitario = produto.Preco
            });
        }

        public async Task FinalizarVendaAsync(string formaPagamento)
        {
            if (Itens.Count == 0)
                throw new InvalidOperationException(
                    "O carrinho está vazio.");

            if (string.IsNullOrWhiteSpace(formaPagamento))
                throw new ArgumentException(
                    "Informe a forma de pagamento.");

            using var context = new AppDbContext();

            var venda = new Venda
            {
                Data = DateTime.Now,
                Total = Total,
                FormaPagamento = formaPagamento
            };

            foreach (var item in Itens)
            {
                venda.Itens.Add(new ItemVenda
                {
                    ProdutoId = item.Produto.Id,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario
                });
            }

            context.Vendas.Add(venda);

            await context.SaveChangesAsync();

            Limpar();
        }

        public void AdicionarProdutoPorPeso(
            Produto produto,
            decimal quantidade)
        {
            if (!produto.VendidoPorPeso)
                throw new InvalidOperationException(
                    "Este produto não é vendido por peso.");

            if (quantidade <= 0)
                throw new ArgumentException(
                    "A quantidade deve ser maior que zero.");

            Itens.Add(new ItemCarrinho
            {
                Produto = produto,
                Quantidade = quantidade,
                PrecoUnitario = produto.Preco
            });
        }

        public void RemoverItem(ItemCarrinho item)
        {
            Itens.Remove(item);
        }

        public void Limpar()
        {
            Itens.Clear();
        }

        public async Task<List<int>> ObterAnosComVendasAsync()
        {
            using var context = new AppDbContext();

            return await context.Vendas
                .AsNoTracking()
                .Select(v => v.Data.Year)
                .Distinct()
                .OrderByDescending(ano => ano)
                .ToListAsync();
        }


        public async Task<RelatorioVendasResultado> ObterRelatorioAsync(
            int ano,
            List<int> meses)
        {
            if (meses == null || meses.Count == 0)
                throw new ArgumentException(
                    "Selecione pelo menos um mês.");

            using var context = new AppDbContext();

            var vendas = await context.Vendas
                .AsNoTracking()
                .Where(v =>
                    v.Data.Year == ano &&
                    meses.Contains(v.Data.Month))
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .ToListAsync();


            var resultado = new RelatorioVendasResultado
            {
                QuantidadeVendas = vendas.Count,
                FaturamentoTotal = vendas.Sum(v => v.Total)
            };


            // ==========================================
            // PRODUTOS
            // ==========================================

            resultado.Produtos = vendas
                .SelectMany(v => v.Itens)
                .GroupBy(i => new
                {
                    i.ProdutoId,
                    i.Produto.Nome,
                    i.Produto.Unidade
                })
                .Select(g => new ResumoProdutoVenda
                {
                    Produto = g.Key.Nome,

                    QuantidadeTotal = g.Sum(i => i.Quantidade),

                    Unidade = g.Key.Unidade,

                    ValorTotal = g.Sum(
                        i => i.Quantidade * i.PrecoUnitario)
                })
                .OrderByDescending(p => p.ValorTotal)
                .ToList();


            // ==========================================
            // FORMAS DE PAGAMENTO
            // ==========================================

            resultado.FormasPagamento = vendas
                .GroupBy(v => v.FormaPagamento)
                .Select(g => new ResumoFormaPagamento
                {
                    FormaPagamento = g.Key,

                    ValorTotal = g.Sum(v => v.Total)
                })
                .OrderByDescending(p => p.ValorTotal)
                .ToList();


            return resultado;
        }
    }
}
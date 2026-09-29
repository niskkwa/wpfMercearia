namespace WpfMercearia.Models
{
    public class RelatorioVendasResultado
    {
        public List<ResumoProdutoVenda> Produtos { get; set; } = new();

        public List<ResumoFormaPagamento> FormasPagamento { get; set; } = new();

        public int QuantidadeVendas { get; set; }

        public decimal FaturamentoTotal { get; set; }
    }
}
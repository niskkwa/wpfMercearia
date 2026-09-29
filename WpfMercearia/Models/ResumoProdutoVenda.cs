namespace WpfMercearia.Models
{
    public class ResumoProdutoVenda
    {
        public string Produto { get; set; } = string.Empty;

        public decimal QuantidadeTotal { get; set; }

        public string Unidade { get; set; } = string.Empty;

        public decimal ValorTotal { get; set; }
    }
}

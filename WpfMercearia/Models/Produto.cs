
using System;
using System.Collections.Generic;
using System.Text;

namespace WpfMercearia.Models
{
    public class Produto
    {
        public int Id { get; set; }

        public string? Codigo { get; set; }

        public string Nome { get; set; } = string.Empty;

        public decimal Preco { get; set; }

        public bool VendidoPorPeso { get; set; }

        public string Unidade { get; set; } = "un";

        public ICollection<ItemVenda> ItensVenda { get; set; } = new List<ItemVenda>();
    }
}

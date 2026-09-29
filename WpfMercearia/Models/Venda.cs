
using System;
using System.Collections.Generic;
using System.Text;

namespace WpfMercearia.Models
{
    public class Venda
    {
        public int Id { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;

        public decimal Total { get; set; }

        public string FormaPagamento { get; set; } = string.Empty;

        public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
    }
}

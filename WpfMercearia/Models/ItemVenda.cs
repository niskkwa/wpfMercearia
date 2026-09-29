using System;
using System.ComponentModel.DataAnnotations.Schema;
using WpfMercearia.Models;

namespace WpfMercearia.Models
{
    // Representa uma linha do carrinho (já escaneada ou já pesada)
    public class ItemVenda
    {
        public int Id { get; set; }

        public int VendaId { get; set; }

        public int ProdutoId { get; set; }

        public decimal Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }

        [NotMapped]
        public decimal Subtotal => Math.Round(
            Quantidade * PrecoUnitario, 2);

        public Venda Venda { get; set; } = null!;

        public Produto Produto { get; set; } = null!;
    }
}

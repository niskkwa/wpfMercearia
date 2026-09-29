using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfMercearia.Models
{
    public class ItemCarrinho : INotifyPropertyChanged
    {
        public Produto Produto { get; set; } = null!;

        private decimal _quantidade;
        public decimal Quantidade
        {
            get => _quantidade;
            set
            {
                if (_quantidade == value)
                    return;

                _quantidade = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        private decimal _precoUnitario;
        public decimal PrecoUnitario
        {
            get => _precoUnitario;
            set
            {
                if (_precoUnitario == value)
                    return;

                _precoUnitario = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal Subtotal =>
            Math.Round(Quantidade * PrecoUnitario, 2);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System.Globalization;
using System.Windows;

namespace WpfMercearia
{
    public partial class PagamentoWindow : Window
    {
        private readonly decimal _total;

        public string? FormaPagamento { get; private set; }

        public PagamentoWindow(decimal total)
        {
            InitializeComponent();

            _total = total;

            txtTotalPagamento.Text =
                $"Total: {_total.ToString("C2")}";

            rbDinheiro.IsChecked = true;
        }

        private void FormaPagamento_Checked(
            object sender,
            RoutedEventArgs e)
        {
            if (painelDinheiro == null)
                return;

            painelDinheiro.Visibility =
                rbDinheiro.IsChecked == true
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            if (rbDinheiro.IsChecked != true)
                txtTroco.Text = "R$ 0,00";
        }

        private void txtValorRecebido_TextChanged(
            object sender,
            System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!decimal.TryParse(
                txtValorRecebido.Text.Replace(',', '.'),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal valorRecebido))
            {
                txtTroco.Text = "R$ 0,00";
                return;
            }

            decimal troco = valorRecebido - _total;

            if (troco < 0)
                troco = 0;

            txtTroco.Text = troco.ToString("C2");
        }

        private void btnCancelar_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnFinalizar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (rbDinheiro.IsChecked == true)
            {
                if (!decimal.TryParse(
                    txtValorRecebido.Text.Replace(',', '.'),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal valorRecebido))
                {
                    MessageBox.Show(
                        "Digite um valor recebido válido.",
                        "Atenção",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    txtValorRecebido.Focus();
                    return;
                }

                if (valorRecebido < _total)
                {
                    MessageBox.Show(
                        $"O valor recebido é menor que o total.\n\n" +
                        $"Total: {_total:C2}\n" +
                        $"Recebido: {valorRecebido:C2}",
                        "Atenção",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    txtValorRecebido.Focus();
                    return;
                }

                FormaPagamento = "Dinheiro";
            }
            else if (rbPix.IsChecked == true)
            {
                FormaPagamento = "PIX";
            }
            else if (rbCartao.IsChecked == true)
            {
                FormaPagamento = "Cartão";
            }

            DialogResult = true;
        }
    }
}
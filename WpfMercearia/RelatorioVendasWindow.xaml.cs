using System.Windows;
using System.Windows.Controls;
using WpfMercearia.Models;
using WpfMercearia.Services;

namespace WpfMercearia
{
    public partial class RelatorioVendasWindow : Window
    {
        private readonly VendaService _vendaService;


        public RelatorioVendasWindow()
        {
            InitializeComponent();

            _vendaService = new VendaService();

            Loaded += RelatorioVendasWindow_Loaded;
        }


        private async void RelatorioVendasWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CarregarAnosAsync();
        }


        // ==========================================
        // CARREGAR ANOS
        // ==========================================

        private async Task CarregarAnosAsync()
        {
            try
            {
                var anos =
                    await _vendaService.ObterAnosComVendasAsync();

                cbAno.ItemsSource = anos;

                if (anos.Count > 0)
                {
                    cbAno.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro ao carregar anos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ==========================================
        // TODOS OS MESES
        // ==========================================

        private void btnTodosMeses_Click(
            object sender,
            RoutedEventArgs e)
        {
            foreach (var checkBox in ObterCheckBoxesMeses())
            {
                checkBox.IsChecked = true;
            }
        }


        // ==========================================
        // LIMPAR MESES
        // ==========================================

        private void btnLimparMeses_Click(
            object sender,
            RoutedEventArgs e)
        {
            foreach (var checkBox in ObterCheckBoxesMeses())
            {
                checkBox.IsChecked = false;
            }
        }


        // ==========================================
        // GERAR RELATÓRIO
        // ==========================================

        private async void btnGerarRelatorio_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (cbAno.SelectedItem == null)
                {
                    MessageBox.Show(
                        "Selecione um ano.",
                        "Atenção",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                var meses = ObterMesesSelecionados();


                if (meses.Count == 0)
                {
                    MessageBox.Show(
                        "Selecione pelo menos um mês.",
                        "Atenção",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                int ano = (int)cbAno.SelectedItem;


                var resultado =
                    await _vendaService.ObterRelatorioAsync(
                        ano,
                        meses);


                // Produtos

                dgProdutos.ItemsSource =
                    resultado.Produtos;


                // Formas de pagamento

                dgFormasPagamento.ItemsSource =
                    resultado.FormasPagamento;


                // Resumo

                txtQuantidadeVendas.Text =
                    resultado.QuantidadeVendas.ToString();


                txtQuantidadeProdutos.Text =
                    resultado.Produtos
                        .Sum(p => p.QuantidadeTotal)
                        .ToString("N3");


                txtFaturamento.Text =
                    resultado.FaturamentoTotal
                        .ToString("C2");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro ao gerar relatório",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ==========================================
        // MESES SELECIONADOS
        // ==========================================

        private List<int> ObterMesesSelecionados()
        {
            return ObterCheckBoxesMeses()
                .Where(c => c.IsChecked == true)
                .Select(c => int.Parse(c.Tag.ToString()!))
                .ToList();
        }


        // ==========================================
        // CHECKBOXES DOS MESES
        // ==========================================

        private List<CheckBox> ObterCheckBoxesMeses()
        {
            return new List<CheckBox>
            {
                chkJaneiro,
                chkFevereiro,
                chkMarco,
                chkAbril,
                chkMaio,
                chkJunho,
                chkJulho,
                chkAgosto,
                chkSetembro,
                chkOutubro,
                chkNovembro,
                chkDezembro
            };
        }
    }
}

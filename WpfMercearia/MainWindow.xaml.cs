using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfMercearia.Models;
using WpfMercearia.Services;

namespace WpfMercearia
{
    public partial class MainWindow : Window
    {
        private readonly ProdutoService _produtoService;
        private readonly VendaService _vendaService;

        // Lista com todos os produtos vendidos por peso
        private List<Produto> _produtosPorPeso = new();

        // Produto selecionado na busca
        private Produto? _frutaSelecionada;

        // Produtos por unidade sem código de barras
        private List<Produto> _produtosSemCodigo = new();

        // Produto selecionado na busca sem código
        private Produto? _produtoSemCodigoSelecionado;
        public MainWindow()
        {
            InitializeComponent();

            _produtoService = new ProdutoService();
            _vendaService = new VendaService();

            CarregarProdutosPorPesoAsync();
            CarregarProdutosSemCodigoAsync();
            AtualizarCarrinho();
        }

        private async void CarregarProdutosSemCodigoAsync()
        {
            _produtosSemCodigo =
                await _produtoService.BuscarProdutosSemCodigoAsync();
        }


        // =========================================================
        // CARREGAR PRODUTOS VENDIDOS POR PESO
        // =========================================================

        private async void CarregarProdutosPorPesoAsync()
        {
            try
            {
                _produtosPorPeso = await _produtoService
                    .BuscarProdutosPorPesoAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro ao carregar produtos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // LEITURA DO CÓDIGO DE BARRAS
        // =========================================================

        private async void txtCodigoBarras_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            string codigo = txtCodigoBarras.Text.Trim();

            if (string.IsNullOrWhiteSpace(codigo))
                return;

            Produto? produto = await _produtoService
                .BuscarPorCodigoAsync(codigo);

            if (produto == null)
            {
                MessageBox.Show(
                    "Produto não encontrado.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtCodigoBarras.Clear();
                txtCodigoBarras.Focus();

                return;
            }

            try
            {
                if (produto.VendidoPorPeso)
                {
                    MessageBox.Show(
                        $"Produto vendido por peso:\n\n" +
                        $"{produto.Nome}\n" +
                        $"Preço: R$ {produto.Preco:F2}/{produto.Unidade}",
                        "Produto por peso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Entrada de peso será feita pela área
                    // de pesagem manual.
                }
                else
                {
                    _vendaService.AdicionarProduto(produto);

                    AtualizarCarrinho();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            txtCodigoBarras.Clear();
            txtCodigoBarras.Focus();
        }

        // =========================================================
        // ATUALIZAR CARRINHO
        // =========================================================

        private void AtualizarCarrinho()
        {
            dgCarrinho.ItemsSource = null;
            dgCarrinho.ItemsSource = _vendaService.Itens;

            txtTotal.Text = _vendaService.Total.ToString("C2");
        }

        // =========================================================
        // BOTÃO PRODUTOS
        // =========================================================

        private void btnProdutos_Click(
            object sender,
            RoutedEventArgs e)
        {
            var janela = new CadastroProdutosWindow
            {
                Owner = this
            };

            janela.ShowDialog();

            // Recarrega os produtos por peso caso algum
            // produto tenha sido cadastrado ou alterado.
            CarregarProdutosPorPesoAsync();
            CarregarProdutosSemCodigoAsync();
        }

        // =========================================================
        // BOTÃO RELATÓRIOS
        // =========================================================

        private void btnRelatorios_Click(
            object sender,
            RoutedEventArgs e)
        {
            var janela = new RelatorioVendasWindow
            {
                Owner = this
            };

            janela.ShowDialog();
        }

        // =========================================================
        // BUSCA DE FRUTAS POR NOME
        // =========================================================

        private void txtBuscaFruta_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            string busca = txtBuscaFruta.Text.Trim();

            // Se não digitou nada, esconde a lista
            if (string.IsNullOrWhiteSpace(busca))
            {
                lstFrutas.ItemsSource = null;
                lstFrutas.Visibility = Visibility.Collapsed;

                _frutaSelecionada = null;

                return;
            }

            // Procura pelo nome
            var resultados = _produtosPorPeso
                .Where(p =>
                    !string.IsNullOrWhiteSpace(p.Nome) &&
                    p.Nome.Contains(
                        busca,
                        StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();

            // Coloca os resultados na lista
            lstFrutas.ItemsSource = resultados;

            // Mostra a lista somente se encontrou resultados
            if (resultados.Count > 0)
            {
                lstFrutas.Visibility = Visibility.Visible;
            }
            else
            {
                lstFrutas.Visibility = Visibility.Collapsed;
            }
        }

        // =========================================================
        // SELECIONAR FRUTA
        // =========================================================

        private void lstFrutas_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (lstFrutas.SelectedItem is not Produto produto)
                return;

            // Guarda o produto selecionado
            _frutaSelecionada = produto;

            // Mostra o nome no campo de busca
            txtBuscaFruta.Text = produto.Nome;

            // Esconde a lista
            lstFrutas.Visibility = Visibility.Collapsed;
        }

        // =========================================================
        // ADICIONAR PRODUTO POR PESO
        // =========================================================

        private void btnAdicionarPeso_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Verifica se uma fruta foi selecionada
            if (_frutaSelecionada == null)
            {
                MessageBox.Show(
                    "Selecione uma fruta.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtBuscaFruta.Focus();

                return;
            }

            // Tenta converter o peso
            if (!decimal.TryParse(
                txtPeso.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal peso))
            {
                MessageBox.Show(
                    "Digite um peso válido.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtPeso.Focus();

                return;
            }

            try
            {
                // Adiciona ao carrinho
                _vendaService.AdicionarProdutoPorPeso(
                    _frutaSelecionada,
                    peso);

                // Atualiza o carrinho e o total
                AtualizarCarrinho();

                // Limpa os campos
                txtPeso.Clear();
                txtBuscaFruta.Clear();

                // Limpa a fruta selecionada
                _frutaSelecionada = null;

                // Limpa a lista de resultados
                lstFrutas.ItemsSource = null;
                lstFrutas.Visibility = Visibility.Collapsed;

                // Volta o foco para a busca da fruta
                txtBuscaFruta.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // REMOVER ITEM DO CARRINHO
        // =========================================================

        private void btnRemoverItem_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgCarrinho.SelectedItem is not ItemCarrinho item)
            {
                MessageBox.Show(
                    "Selecione um item para remover.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            _vendaService.RemoverItem(item);

            AtualizarCarrinho();
        }

        // =========================================================
        // CANCELAR VENDA
        // =========================================================

        private void btnCancelarVenda_Click(
            object sender,
            RoutedEventArgs e)
        {
            _vendaService.Limpar();

            AtualizarCarrinho();

            txtBuscaFruta.Clear();
            txtPeso.Clear();

            _frutaSelecionada = null;

            lstFrutas.ItemsSource = null;
            lstFrutas.Visibility = Visibility.Collapsed;

            txtCodigoBarras.Focus();
        }

        // =========================================================
        // FINALIZAR VENDA
        // =========================================================

        private async void btnFinalizarVenda_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_vendaService.Itens.Count == 0)
            {
                MessageBox.Show(
                    "O carrinho está vazio.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var pagamentoWindow = new PagamentoWindow(
                _vendaService.Total)
            {
                Owner = this
            };

            bool? resultado = pagamentoWindow.ShowDialog();

            if (resultado != true)
                return;

            try
            {
                await _vendaService.FinalizarVendaAsync(
                    pagamentoWindow.FormaPagamento!);

                AtualizarCarrinho();

                MessageBox.Show(
                    "Venda finalizada com sucesso!",
                    "Venda",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                txtCodigoBarras.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro ao finalizar venda",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void txtBuscaProduto_TextChanged(
        object sender,
         TextChangedEventArgs e)
        {
            string busca = txtBuscaProduto.Text.Trim();

            if (string.IsNullOrWhiteSpace(busca))
            {
                lstProdutos.ItemsSource = null;
                lstProdutos.Visibility = Visibility.Collapsed;

                _produtoSemCodigoSelecionado = null;

                return;
            }

            var resultados = _produtosSemCodigo
                .Where(p =>
                    !string.IsNullOrWhiteSpace(p.Nome) &&
                    p.Nome.Contains(
                        busca,
                        StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();

            lstProdutos.ItemsSource = resultados;

            if (resultados.Count > 0)
            {
                lstProdutos.Visibility = Visibility.Visible;
            }
            else
            {
                lstProdutos.Visibility = Visibility.Collapsed;
            }
        }

        private void lstProdutos_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
        {
            if (lstProdutos.SelectedItem is not Produto produto)
                return;

            _produtoSemCodigoSelecionado = produto;

            txtBuscaProduto.Text = produto.Nome;

            lstProdutos.Visibility = Visibility.Collapsed;
        }

        private void btnAdicionarProduto_Click(
        object sender,
        RoutedEventArgs e)
        {
            if (_produtoSemCodigoSelecionado == null)
            {
                MessageBox.Show(
                    "Selecione um produto.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtBuscaProduto.Focus();

                return;
            }

            try
            {
                _vendaService.AdicionarProduto(
                    _produtoSemCodigoSelecionado);

                AtualizarCarrinho();

                _produtoSemCodigoSelecionado = null;

                txtBuscaProduto.Clear();

                lstProdutos.ItemsSource = null;
                lstProdutos.Visibility = Visibility.Collapsed;

                txtBuscaProduto.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void btnSair_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

}


using System;
using System.Windows;
using WpfMercearia.Models;
using WpfMercearia.Services;

namespace WpfMercearia
{
    public partial class CadastroProdutosWindow : Window
    {
        private readonly ProdutoService _produtoService;


    private Produto? _produtoEmEdicao;


        public CadastroProdutosWindow()
        {
            InitializeComponent();

            _produtoService = new ProdutoService();

            CarregarProdutosAsync();
        }


        private async void CarregarProdutosAsync()
        {
            try
            {
                var produtos =
                    await _produtoService.ListarProdutosAsync();

                dgProdutos.ItemsSource = produtos;
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


        // ==========================================
        // CADASTRAR PRODUTO
        // ==========================================

        private async void btnCadastrar_Click(
         object sender,
          RoutedEventArgs e)
        {
            try
            {
                string codigo = txtCodigo.Text.Trim();
                string nome = txtNome.Text.Trim();
                string precoTexto = txtPreco.Text.Trim();

                bool vendidoPorPeso =
                    chkVendidoPorPeso.IsChecked == true;


                // =========================
                // VALIDA NOME
                // =========================

                if (string.IsNullOrWhiteSpace(nome))
                {
                    MessageBox.Show(
                        "Informe o nome do produto.",
                        "Atenção",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    txtNome.Focus();
                    return;
                }


                // =========================
                // VALIDA PREÇO
                // =========================

                if (!decimal.TryParse(
                    precoTexto,
                    out decimal preco))
                {
                    MessageBox.Show(
                        "Informe um preço válido.",
                        "Atenção",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    txtPreco.Focus();
                    return;
                }


                if (preco <= 0)
                {
                    MessageBox.Show(
                        "O preço deve ser maior que zero.",
                        "Atenção",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    txtPreco.Focus();
                    return;
                }


                // =========================
                // VERIFICA CÓDIGO DUPLICADO
                // =========================

                if (!string.IsNullOrWhiteSpace(codigo))
                {
                    var produtoExistente =
                        await _produtoService
                            .BuscarPorCodigoAsync(codigo);

                    if (produtoExistente != null)
                    {
                        MessageBox.Show(
                            "Já existe um produto cadastrado com este código.",
                            "Código duplicado",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        txtCodigo.Focus();
                        return;
                    }
                }


                // =========================
                // CRIA PRODUTO
                // =========================

                var produto = new Produto
                {
                    Codigo = string.IsNullOrWhiteSpace(codigo)
                        ? null
                        : codigo,

                    Nome = nome,

                    Preco = preco,

                    VendidoPorPeso = vendidoPorPeso,

                    Unidade = vendidoPorPeso
                        ? "kg"
                        : "un"
                };


                await _produtoService
                    .CadastrarProdutoAsync(produto);


                MessageBox.Show(
                    "Produto cadastrado com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                LimparCampos();

                CarregarProdutosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro ao cadastrar produto",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ==========================================
        // EDITAR PRODUTO
        // ==========================================

        private void btnEditar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgProdutos.SelectedItem is not Produto produto)
            {
                MessageBox.Show(
                    "Selecione um produto para editar.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            _produtoEmEdicao = produto;


            txtCodigo.Text =
                produto.Codigo ?? string.Empty;

            txtNome.Text =
                produto.Nome;

            txtPreco.Text =
                produto.Preco.ToString("F2");

            chkVendidoPorPeso.IsChecked =
                produto.VendidoPorPeso;

            txtUnidade.Text =
                produto.VendidoPorPeso
                    ? "kg"
                    : "un";


            btnCadastrar.Visibility =
                Visibility.Collapsed;

            btnSalvarEdicao.Visibility =
                Visibility.Visible;

            btnCancelarEdicao.Visibility =
                Visibility.Visible;


            txtCodigo.IsEnabled =
                !produto.VendidoPorPeso;


            txtNome.Focus();
        }


        // ==========================================
        // SALVAR ALTERAÇÕES
        // ==========================================

       
        private async void btnSalvarEdicao_Click(
        object sender,
        RoutedEventArgs e)
        {
            if (_produtoEmEdicao == null)
                return;

            try
            {
                string codigo =
                    txtCodigo.Text.Trim();

                string nome =
                    txtNome.Text.Trim();

                string precoTexto =
                    txtPreco.Text.Trim();

                bool vendidoPorPeso =
                    chkVendidoPorPeso.IsChecked == true;


                // ==========================================
                // VALIDA NOME
                // ==========================================

                if (string.IsNullOrWhiteSpace(nome))
                {
                    MessageBox.Show(
                        "Informe o nome do produto.",
                        "Atenção",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    txtNome.Focus();
                    return;
                }


                // ==========================================
                // VALIDA PREÇO
                // ==========================================

                if (!decimal.TryParse(
                    precoTexto,
                    out decimal preco))
                {
                    MessageBox.Show(
                        "Informe um preço válido.",
                        "Atenção",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    txtPreco.Focus();
                    return;
                }


                if (preco <= 0)
                {
                    MessageBox.Show(
                        "O preço deve ser maior que zero.",
                        "Atenção",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    txtPreco.Focus();
                    return;
                }


                // ==========================================
                // VERIFICA CÓDIGO DUPLICADO
                // ==========================================

                // Só verifica duplicidade se foi informado
                // algum código.

                if (!string.IsNullOrWhiteSpace(codigo))
                {
                    var produtoComMesmoCodigo =
                        await _produtoService
                            .BuscarPorCodigoAsync(codigo);


                    // Ignora o próprio produto que está sendo editado
                    if (produtoComMesmoCodigo != null &&
                        produtoComMesmoCodigo.Id !=
                        _produtoEmEdicao.Id)
                    {
                        MessageBox.Show(
                            "Já existe outro produto cadastrado com este código.",
                            "Código duplicado",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        txtCodigo.Focus();
                        return;
                    }
                }


                // ==========================================
                // ATUALIZA PRODUTO
                // ==========================================

                _produtoEmEdicao.Codigo =
                    string.IsNullOrWhiteSpace(codigo)
                        ? null
                        : codigo;

                _produtoEmEdicao.Nome =
                    nome;

                _produtoEmEdicao.Preco =
                    preco;

                _produtoEmEdicao.VendidoPorPeso =
                    vendidoPorPeso;

                _produtoEmEdicao.Unidade =
                    vendidoPorPeso
                        ? "kg"
                        : "un";


                // ==========================================
                // SALVA NO BANCO
                // ==========================================

                await _produtoService
                    .AtualizarProdutoAsync(
                        _produtoEmEdicao);


                MessageBox.Show(
                    "Produto atualizado com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                // ==========================================
                // LIMPA E RECARREGA
                // ==========================================

                CancelarEdicao();

                CarregarProdutosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro ao atualizar produto",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }




        // ==========================================
        // CANCELAR EDIÇÃO
        // ==========================================

        private void btnCancelarEdicao_Click(
            object sender,
            RoutedEventArgs e)
        {
            CancelarEdicao();
        }


        private void CancelarEdicao()
        {
            _produtoEmEdicao = null;


            btnCadastrar.Visibility =
                Visibility.Visible;

            btnSalvarEdicao.Visibility =
                Visibility.Collapsed;

            btnCancelarEdicao.Visibility =
                Visibility.Collapsed;


            txtCodigo.IsEnabled = true;


            LimparCampos();
        }


        // ==========================================
        // EXCLUIR PRODUTO
        // ==========================================

        private async void btnExcluir_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgProdutos.SelectedItem is not Produto produto)
            {
                MessageBox.Show(
                    "Selecione um produto para excluir.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            var resposta = MessageBox.Show(
                $"Deseja realmente excluir o produto \"{produto.Nome}\"?",
                "Confirmar exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);


            if (resposta != MessageBoxResult.Yes)
                return;


            try
            {
                await _produtoService
                    .ExcluirProdutoAsync(produto.Id);


                MessageBox.Show(
                    "Produto excluído com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                CarregarProdutosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro ao excluir produto",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ==========================================
        // LIMPAR CAMPOS
        // ==========================================

        private void LimparCampos()
        {
            txtCodigo.Clear();

            txtNome.Clear();

            txtPreco.Clear();

            txtUnidade.Text = "un";

            chkVendidoPorPeso.IsChecked = false;

            txtCodigo.IsEnabled = true;

            txtCodigo.Focus();
        }


        // ==========================================
        // PRODUTO VENDIDO POR PESO
        // ==========================================

        private void chkVendidoPorPeso_Checked(
            object sender,
            RoutedEventArgs e)
        {
            txtUnidade.Text = "kg";

            txtCodigo.Clear();

            if (_produtoEmEdicao != null)
            {
                txtCodigo.IsEnabled = false;
            }
        }


        private void chkVendidoPorPeso_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            txtUnidade.Text = "un";

            txtCodigo.IsEnabled = true;
        }
    }

}

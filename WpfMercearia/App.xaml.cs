
using System.Windows;
using Microsoft.EntityFrameworkCore;
using WpfMercearia.Data;

namespace WpfMercearia
{
    public partial class App : Application
    {
        protected override void OnStartup(
            StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using var context = new AppDbContext();

                // Cria o banco e aplica as migrations automaticamente
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Não foi possível inicializar o banco de dados.\n\n{ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
            }
        }
    }
}



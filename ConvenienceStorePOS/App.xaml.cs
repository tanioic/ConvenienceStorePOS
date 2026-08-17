using System.Windows;
using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Services;
using ConvenienceStorePOS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ConvenienceStorePOS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Initialize database & view models
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            await mainViewModel.InitializeAsync();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Data Layer
            services.AddSingleton<IDatabaseInitializer, SqliteDatabaseInitializer>();
            services.AddSingleton<IProductRepository, SqliteProductRepository>();
            services.AddSingleton<ISaleRepository, SqliteSaleRepository>();

            // Business Logic Services
            services.AddSingleton<IProductService, ProductService>();
            services.AddSingleton<ISaleService, SaleService>();
            services.AddSingleton<IAccountingService, AccountingService>();
            services.AddSingleton<IReceiptService, ReceiptService>();

            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<ProductManagementViewModel>();
            services.AddTransient<SalesReportViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<ProductManagementWindow>();
            services.AddTransient<SalesReportWindow>();
        }
    }
}

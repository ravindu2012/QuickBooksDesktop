using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;
using QBD.Infrastructure.Data;
using QBD.Infrastructure.Repositories;
using QBD.Infrastructure.Services;
using QBD.WPF.Services;

namespace QBD.WPF;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Database
                services.AddDbContext<QBDesktopDbContext>(options =>
                    options.UseSqlServer(
                        "Server=(localdb)\\MSSQLLocalDB;Database=QuickBooksDesktop;Trusted_Connection=true;TrustServerCertificate=true;"));

                // Repositories
                services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                // Services
                services.AddScoped<ITransactionPostingService, TransactionPostingService>();
                services.AddScoped<INumberSequenceService, NumberSequenceService>();
                services.AddScoped<IAuditService, AuditService>();

                // Navigation
                services.AddSingleton<NavigationService>();
                services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<NavigationService>());

                // Seeder
                services.AddScoped<DatabaseSeeder>();

                // Shell
                services.AddSingleton<MainWindow>();

                // Core ViewModels
                services.AddTransient<HomePageViewModel>();

                // Company Module
                services.AddTransient<QBD.Modules.Company.ViewModels.CompanyInfoFormViewModel>();
                services.AddTransient<QBD.Modules.Company.ViewModels.PreferencesFormViewModel>();
                services.AddTransient<QBD.Modules.Company.ViewModels.ChartOfAccountsViewModel>();
                services.AddTransient<QBD.Modules.Company.ViewModels.ItemListViewModel>();
                services.AddTransient<QBD.Modules.Company.ViewModels.ClassListViewModel>();
                services.AddTransient<QBD.Modules.Company.ViewModels.TermsListViewModel>();
                services.AddTransient<QBD.Modules.Company.ViewModels.PaymentMethodListViewModel>();

                // Customers Module
                services.AddTransient<QBD.Modules.Customers.ViewModels.CustomerCenterViewModel>();
                services.AddTransient<QBD.Modules.Customers.ViewModels.InvoiceFormViewModel>();
                services.AddTransient<QBD.Modules.Customers.ViewModels.ReceivePaymentFormViewModel>();
                services.AddTransient<QBD.Modules.Customers.ViewModels.SalesReceiptFormViewModel>();
                services.AddTransient<QBD.Modules.Customers.ViewModels.CreditMemoFormViewModel>();
                services.AddTransient<QBD.Modules.Customers.ViewModels.EstimateFormViewModel>();

                // Vendors Module
                services.AddTransient<QBD.Modules.Vendors.ViewModels.VendorCenterViewModel>();
                services.AddTransient<QBD.Modules.Vendors.ViewModels.BillFormViewModel>();
                services.AddTransient<QBD.Modules.Vendors.ViewModels.PayBillsFormViewModel>();
                services.AddTransient<QBD.Modules.Vendors.ViewModels.PurchaseOrderFormViewModel>();
                services.AddTransient<QBD.Modules.Vendors.ViewModels.VendorCreditFormViewModel>();

                // Banking Module
                services.AddTransient<QBD.Modules.Banking.ViewModels.WriteChecksFormViewModel>();
                services.AddTransient<QBD.Modules.Banking.ViewModels.MakeDepositsFormViewModel>();
                services.AddTransient<QBD.Modules.Banking.ViewModels.TransferFundsFormViewModel>();
                services.AddTransient<QBD.Modules.Banking.ViewModels.BankRegisterViewModel>();
                services.AddTransient<QBD.Modules.Banking.ViewModels.ReconcileViewModel>();

                // Reports Module
                services.AddTransient<QBD.Modules.Reports.ViewModels.ReportCenterViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.ProfitLossReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.BalanceSheetReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.CashFlowsReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.TrialBalanceReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.GeneralLedgerReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.ARAgingSummaryReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.ARAgingDetailReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.APAgingSummaryReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.APAgingDetailReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.CustomerBalanceReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.VendorBalanceReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.OpenInvoicesReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.UnpaidBillsReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.TransactionListReportViewModel>();
                services.AddTransient<QBD.Modules.Reports.ViewModels.DepositDetailReportViewModel>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Ensure database is created and seeded
        using (var scope = _host.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<QBDesktopDbContext>();
            await context.Database.EnsureCreatedAsync();

            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
        }

        // Set up navigation service with main window
        var navigationService = _host.Services.GetRequiredService<NavigationService>();
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        navigationService.SetMainWindow(mainWindow);
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}

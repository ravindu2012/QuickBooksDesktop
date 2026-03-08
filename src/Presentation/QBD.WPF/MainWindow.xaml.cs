using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;

namespace QBD.WPF;

public partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;
    private bool _isDarkMode = false;
    private readonly string _themeConfigFile;

    public MainWindow(INavigationService navigationService, HomePageViewModel homePageViewModel)
    {
        InitializeComponent();
        _navigationService = navigationService;

        var appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QBD", "QBD.WPF");
        if (!Directory.Exists(appDataDirectory))
        {
            Directory.CreateDirectory(appDataDirectory);
        }
        _themeConfigFile = Path.Combine(appDataDirectory, "theme.cfg");

        try
        {
            if (File.Exists(_themeConfigFile))
            {
                var configContent = File.ReadAllText(_themeConfigFile);
                if (bool.TryParse(configContent, out bool isDark))
                {
                    _isDarkMode = isDark;
                    var app = (App)System.Windows.Application.Current;
                    app.ChangeTheme(_isDarkMode);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Napaka pri branju teme: {ex.Message}");
        }

        // Open Home Page on startup
        OpenTab(homePageViewModel);
    }

    public void OpenTab(ViewModelBase viewModel)
    {
        foreach (var item in WorkspaceTabs.Items)
        {
            if (item is ViewModelBase existing && existing.GetType() == viewModel.GetType()
                && existing.Title == viewModel.Title)
            {
                WorkspaceTabs.SelectedItem = item;
                return;
            }
        }
        WorkspaceTabs.Items.Add(viewModel);
        WorkspaceTabs.SelectedItem = viewModel;
    }

    public void CloseTab(object viewModel)
    {
        WorkspaceTabs.Items.Remove(viewModel);
    }

    public void CloseAllTabs()
    {
        WorkspaceTabs.Items.Clear();
    }

    // Event handlers delegate to navigation service
    private void Exit_Click(object sender, RoutedEventArgs e) => System.Windows.Application.Current.Shutdown();
    private void HomePage_Click(object sender, RoutedEventArgs e) => _navigationService.OpenHomePage();
    private void ChartOfAccounts_Click(object sender, RoutedEventArgs e) => _navigationService.OpenList("ChartOfAccounts");
    private void ItemList_Click(object sender, RoutedEventArgs e) => _navigationService.OpenList("Items");
    private void ClassList_Click(object sender, RoutedEventArgs e) => _navigationService.OpenList("Classes");
    private void TermsList_Click(object sender, RoutedEventArgs e) => _navigationService.OpenList("Terms");
    private void PaymentMethodList_Click(object sender, RoutedEventArgs e) => _navigationService.OpenList("PaymentMethods");
    private void CompanyInfo_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("CompanyInfo");
    private void Preferences_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("Preferences");
    private void CustomerCenter_Click(object sender, RoutedEventArgs e) => _navigationService.OpenCenter("CustomerCenter");
    private void CreateInvoice_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("Invoice");
    private void CreateEstimate_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("Estimate");
    private void CreateSalesReceipt_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("SalesReceipt");
    private void CreateCreditMemo_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("CreditMemo");
    private void ReceivePayment_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("ReceivePayment");
    private void VendorCenter_Click(object sender, RoutedEventArgs e) => _navigationService.OpenCenter("VendorCenter");
    private void EnterBill_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("Bill");
    private void PayBills_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("PayBills");
    private void CreatePurchaseOrder_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("PurchaseOrder");
    private void EnterVendorCredit_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("VendorCredit");
    private void WriteChecks_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("Check");
    private void MakeDeposits_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("Deposit");
    private void TransferFunds_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("Transfer");
    private void Reconcile_Click(object sender, RoutedEventArgs e) => _navigationService.OpenForm("Reconcile");
    private void UseRegister_Click(object sender, RoutedEventArgs e) => _navigationService.OpenRegister(0);
    private void ReportCenter_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("ReportCenter");
    private void ProfitLoss_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("ProfitLoss");
    private void BalanceSheet_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("BalanceSheet");
    private void CashFlows_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("CashFlows");
    private void TrialBalance_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("TrialBalance");
    private void GeneralLedger_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("GeneralLedger");
    private void ARAgingSummary_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("ARAgingSummary");
    private void ARAgingDetail_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("ARAgingDetail");
    private void CustomerBalance_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("CustomerBalance");
    private void OpenInvoices_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("OpenInvoices");
    private void APAgingSummary_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("APAgingSummary");
    private void APAgingDetail_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("APAgingDetail");
    private void VendorBalance_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("VendorBalance");
    private void UnpaidBills_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("UnpaidBills");
    private void DepositDetail_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("DepositDetail");
    private void TransactionListByDate_Click(object sender, RoutedEventArgs e) => _navigationService.OpenReport("TransactionListByDate");

    private void CloseTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ViewModelBase vm)
        {
            CloseTab(vm);
        }
    }

    private void CloseAllTabs_Click(object sender, RoutedEventArgs e) => CloseAllTabs();

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("QuickBooks Desktop Enterprise Clone\nVersion 1.0\n\nA full-featured accounting application.",
            "About", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ToggleDarkMode_Click(object sender, RoutedEventArgs e)
    {
        _isDarkMode = !_isDarkMode;
        var app = (App)System.Windows.Application.Current;
        app.ChangeTheme(_isDarkMode);

        try
        {
            File.WriteAllText(_themeConfigFile, _isDarkMode.ToString());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Napaka pri shranjevanju teme: {ex.Message}");
        }
    }
}
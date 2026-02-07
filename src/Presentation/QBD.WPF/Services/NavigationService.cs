using QBD.Application.Interfaces;
using QBD.Application.ViewModels;

namespace QBD.WPF.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private MainWindow? _mainWindow;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void SetMainWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void OpenTab(object viewModel)
    {
        if (viewModel is ViewModelBase vm)
        {
            _mainWindow?.OpenTab(vm);
            _ = vm.InitializeAsync();
        }
    }

    public void CloseTab(object viewModel)
    {
        _mainWindow?.CloseTab(viewModel);
    }

    public void OpenHomePage()
    {
        var vm = GetService<HomePageViewModel>();
        OpenTab(vm);
    }

    public void OpenCenter(string centerName)
    {
        ViewModelBase? vm = centerName switch
        {
            "CustomerCenter" => GetService<QBD.Modules.Customers.ViewModels.CustomerCenterViewModel>(),
            "VendorCenter" => GetService<QBD.Modules.Vendors.ViewModels.VendorCenterViewModel>(),
            _ => null
        };
        if (vm != null) OpenTab(vm);
    }

    public void OpenForm(string formName, int? entityId = null)
    {
        ViewModelBase? vm = formName switch
        {
            "Invoice" => GetService<QBD.Modules.Customers.ViewModels.InvoiceFormViewModel>(),
            "Estimate" => GetService<QBD.Modules.Customers.ViewModels.EstimateFormViewModel>(),
            "SalesReceipt" => GetService<QBD.Modules.Customers.ViewModels.SalesReceiptFormViewModel>(),
            "CreditMemo" => GetService<QBD.Modules.Customers.ViewModels.CreditMemoFormViewModel>(),
            "ReceivePayment" => GetService<QBD.Modules.Customers.ViewModels.ReceivePaymentFormViewModel>(),
            "Bill" => GetService<QBD.Modules.Vendors.ViewModels.BillFormViewModel>(),
            "PayBills" => GetService<QBD.Modules.Vendors.ViewModels.PayBillsFormViewModel>(),
            "PurchaseOrder" => GetService<QBD.Modules.Vendors.ViewModels.PurchaseOrderFormViewModel>(),
            "VendorCredit" => GetService<QBD.Modules.Vendors.ViewModels.VendorCreditFormViewModel>(),
            "Check" => GetService<QBD.Modules.Banking.ViewModels.WriteChecksFormViewModel>(),
            "Deposit" => GetService<QBD.Modules.Banking.ViewModels.MakeDepositsFormViewModel>(),
            "Transfer" => GetService<QBD.Modules.Banking.ViewModels.TransferFundsFormViewModel>(),
            "Reconcile" => GetService<QBD.Modules.Banking.ViewModels.ReconcileViewModel>(),
            "CompanyInfo" => GetService<QBD.Modules.Company.ViewModels.CompanyInfoFormViewModel>(),
            "Preferences" => GetService<QBD.Modules.Company.ViewModels.PreferencesFormViewModel>(),
            _ => null
        };
        if (vm != null) OpenTab(vm);
    }

    public void OpenRegister(int accountId)
    {
        var vm = GetService<QBD.Modules.Banking.ViewModels.BankRegisterViewModel>();
        OpenTab(vm);
    }

    public void OpenReport(string reportName)
    {
        ViewModelBase? vm = reportName switch
        {
            "ReportCenter" => GetService<QBD.Modules.Reports.ViewModels.ReportCenterViewModel>(),
            "ProfitLoss" => GetService<QBD.Modules.Reports.ViewModels.ProfitLossReportViewModel>(),
            "BalanceSheet" => GetService<QBD.Modules.Reports.ViewModels.BalanceSheetReportViewModel>(),
            "CashFlows" => GetService<QBD.Modules.Reports.ViewModels.CashFlowsReportViewModel>(),
            "TrialBalance" => GetService<QBD.Modules.Reports.ViewModels.TrialBalanceReportViewModel>(),
            "GeneralLedger" => GetService<QBD.Modules.Reports.ViewModels.GeneralLedgerReportViewModel>(),
            "ARAgingSummary" => GetService<QBD.Modules.Reports.ViewModels.ARAgingSummaryReportViewModel>(),
            "ARAgingDetail" => GetService<QBD.Modules.Reports.ViewModels.ARAgingDetailReportViewModel>(),
            "APAgingSummary" => GetService<QBD.Modules.Reports.ViewModels.APAgingSummaryReportViewModel>(),
            "APAgingDetail" => GetService<QBD.Modules.Reports.ViewModels.APAgingDetailReportViewModel>(),
            "CustomerBalance" => GetService<QBD.Modules.Reports.ViewModels.CustomerBalanceReportViewModel>(),
            "VendorBalance" => GetService<QBD.Modules.Reports.ViewModels.VendorBalanceReportViewModel>(),
            "OpenInvoices" => GetService<QBD.Modules.Reports.ViewModels.OpenInvoicesReportViewModel>(),
            "UnpaidBills" => GetService<QBD.Modules.Reports.ViewModels.UnpaidBillsReportViewModel>(),
            "TransactionListByDate" => GetService<QBD.Modules.Reports.ViewModels.TransactionListReportViewModel>(),
            "DepositDetail" => GetService<QBD.Modules.Reports.ViewModels.DepositDetailReportViewModel>(),
            _ => null
        };
        if (vm != null) OpenTab(vm);
    }

    public void OpenList(string listName)
    {
        ViewModelBase? vm = listName switch
        {
            "ChartOfAccounts" => GetService<QBD.Modules.Company.ViewModels.ChartOfAccountsViewModel>(),
            "Items" => GetService<QBD.Modules.Company.ViewModels.ItemListViewModel>(),
            "Classes" => GetService<QBD.Modules.Company.ViewModels.ClassListViewModel>(),
            "Terms" => GetService<QBD.Modules.Company.ViewModels.TermsListViewModel>(),
            "PaymentMethods" => GetService<QBD.Modules.Company.ViewModels.PaymentMethodListViewModel>(),
            _ => null
        };
        if (vm != null) OpenTab(vm);
    }

    private T GetService<T>() where T : notnull
    {
        return (T)_serviceProvider.GetService(typeof(T))!;
    }
}

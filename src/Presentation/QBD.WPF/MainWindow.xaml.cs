using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QBD.Application.Interfaces;
using QBD.Application.ViewModels;

namespace QBD.WPF;

public partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;
    private bool _isDarkMode = false;
    private string _themeConfigFile = string.Empty;

    public ICommand HomePageCommand { get; }
    public ICommand FocusSearchCommand { get; }
    public ICommand CloseCurrentTabCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand ShowKeyboardShortcutsCommand { get; }

    public MainWindow(INavigationService navigationService, HomePageViewModel homePageViewModel)
    {
        _navigationService = navigationService;

        HomePageCommand = new ActionCommand(() => _navigationService.OpenHomePage());
        FocusSearchCommand = new ActionCommand(() => FocusSearch());
        CloseCurrentTabCommand = new ActionCommand(() => CloseCurrentTab());
        SaveCommand = new ActionCommand(() => ExecuteGlobalSave());
        NewCommand = new ActionCommand(() => ExecuteGlobalNew());
        ShowKeyboardShortcutsCommand = new ActionCommand(() => ShowKeyboardShortcuts());

        InitializeComponent();

        this.DataContext = this;

        try
        {
            var appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QBD", "QBD.WPF");
            if (!Directory.Exists(appDataDirectory)) Directory.CreateDirectory(appDataDirectory);
            
            _themeConfigFile = Path.Combine(appDataDirectory, "theme.cfg");

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
            System.Diagnostics.Debug.WriteLine($"Error reading theme configuration: {ex.Message}");
        }

        OpenTab(homePageViewModel);
    }

    public void OpenTab(ViewModelBase viewModel)
    {
        foreach (var item in WorkspaceTabs.Items)
        {
            if (item is ViewModelBase existing && existing.GetType() == viewModel.GetType() && existing.Title == viewModel.Title)
            {
                WorkspaceTabs.SelectedItem = item;
                return;
            }
        }
        WorkspaceTabs.Items.Add(viewModel);
        WorkspaceTabs.SelectedItem = viewModel;
    }

    public void CloseTab(object viewModel) => WorkspaceTabs.Items.Remove(viewModel);
    public void CloseAllTabs() => WorkspaceTabs.Items.Clear();

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
        if (sender is Button btn && btn.DataContext is ViewModelBase vm) CloseTab(vm);
    }
    private void CloseAllTabs_Click(object sender, RoutedEventArgs e) => CloseAllTabs();

    private void KeyboardShortcuts_Click(object sender, RoutedEventArgs e) => ShowKeyboardShortcuts();

    private void ShowKeyboardShortcuts()
    {
        var shortcuts = new Window
        {
            Title = "Keyboard Shortcuts",
            Width = 450,
            Height = 480,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize,
            Background = (System.Windows.Media.Brush)FindResource("ThemeWindowBackground")
        };

        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new TextBlock
        {
            Text = "Keyboard Shortcuts",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 12),
            Foreground = (System.Windows.Media.Brush)FindResource("ThemeForeground")
        };
        Grid.SetRow(header, 0);

        var items = new[]
        {
            ("F1", "Show this help dialog"),
            ("F5", "Go to Home Page"),
            ("Ctrl+N", "New transaction (context-aware)"),
            ("Ctrl+S", "Save current form"),
            ("Ctrl+F", "Focus search box"),
            ("Ctrl+A", "Chart of Accounts"),
            ("Esc", "Close current tab"),
            ("Tab", "Move to next field"),
            ("Shift+Tab", "Move to previous field"),
            ("Alt+S", "Save (in forms)"),
            ("Alt+P", "Save & Post (in forms)"),
            ("Alt+L", "Clear (in forms)"),
            ("Alt+V", "Void (in forms)"),
            ("Alt+R", "Print (in forms)"),
            ("Alt+N", "New (in lists)"),
            ("Alt+E", "Edit (in lists)"),
            ("Alt+D", "Delete (in lists)"),
        };

        var listView = new ListView
        {
            BorderThickness = new Thickness(1),
            BorderBrush = (System.Windows.Media.Brush)FindResource("ThemeBorderBrush"),
            Background = (System.Windows.Media.Brush)FindResource("ThemeControlBackground")
        };

        var gridView = new GridView();
        gridView.Columns.Add(new GridViewColumn
        {
            Header = "Shortcut",
            Width = 120,
            DisplayMemberBinding = new System.Windows.Data.Binding("Item1")
        });
        gridView.Columns.Add(new GridViewColumn
        {
            Header = "Action",
            Width = 280,
            DisplayMemberBinding = new System.Windows.Data.Binding("Item2")
        });
        listView.View = gridView;

        foreach (var item in items)
            listView.Items.Add(item);

        Grid.SetRow(listView, 1);

        var closeBtn = new Button
        {
            Content = "Close",
            Padding = new Thickness(20, 6, 20, 6),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0)
        };
        closeBtn.Click += (_, _) => shortcuts.Close();
        Grid.SetRow(closeBtn, 2);

        grid.Children.Add(header);
        grid.Children.Add(listView);
        grid.Children.Add(closeBtn);
        shortcuts.Content = grid;
        shortcuts.ShowDialog();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("QuickBooks Desktop Enterprise Clone\nVersion 1.0\n\nA full-featured accounting application.", "About", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ToggleDarkMode_Click(object sender, RoutedEventArgs e)
    {
        _isDarkMode = !_isDarkMode;
        ((App)System.Windows.Application.Current).ChangeTheme(_isDarkMode);
        try { if (!string.IsNullOrEmpty(_themeConfigFile)) File.WriteAllText(_themeConfigFile, _isDarkMode.ToString()); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error saving theme: {ex.Message}"); }
    }

    private void CloseCurrentTab()
    {
        if (WorkspaceTabs.SelectedItem != null) CloseTab(WorkspaceTabs.SelectedItem);
    }

    private void FocusSearch()
    {
        var contentPresenter = FindVisualChild<ContentPresenter>(WorkspaceTabs);
        var searchRoot = contentPresenter ?? (DependencyObject)WorkspaceTabs;
        var searchBox = FindSearchTextBox(searchRoot);

        if (searchBox != null)
        {
            searchBox.Focus();
            Keyboard.Focus(searchBox);
            StatusText.Text = "Search box focused (Ctrl+F).";
        }
        else
        {
            StatusText.Text = "No search box found in this view.";
        }
    }

    private void ExecuteGlobalNew()
    {
        var selectedItem = WorkspaceTabs.SelectedItem;
        if (selectedItem == null) return;

        StatusText.Text = "Opening new entry form...";

        var itemType = selectedItem.GetType();
        string[] commandNames = { "NewEntityCommand", "NewItemCommand", "NewCommand" };

        foreach (var name in commandNames)
        {
            var prop = itemType.GetProperty(name);
            if (prop != null && prop.GetValue(selectedItem) is ICommand command && command.CanExecute(null))
            {
                command.Execute(null);
                (WorkspaceTabs.SelectedContent as FrameworkElement)?.Focus();
                return;
            }
        }
        StatusText.Text = "New entry not available here.";
    }

    private async void ExecuteGlobalSave()
    {
        var selectedItem = WorkspaceTabs.SelectedItem;
        if (selectedItem == null) return;

        var prop = selectedItem.GetType().GetProperty("SaveCommand");
        if (prop != null && prop.GetValue(selectedItem) is ICommand command && command.CanExecute(null))
        {
            StatusText.Text = "Saving...";
            try 
            {
                var asyncMethod = command.GetType().GetMethod("ExecuteAsync");
                if (asyncMethod != null && asyncMethod.Invoke(command, new object?[] { null }) is Task task)
                {
                    await task;
                }
                else
                {
                    command.Execute(null);
                }
                StatusText.Text = "Transaction saved successfully (Ctrl+S).";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Save failed.";
                System.Diagnostics.Debug.WriteLine($"Save error: {ex.Message}");
            }
            
            (WorkspaceTabs.SelectedContent as FrameworkElement)?.Focus();
        }
        else
        {
            StatusText.Text = "Save not supported in this view.";
        }
    }

    private TextBox? FindSearchTextBox(DependencyObject? obj)
    {
        if (obj == null) return null;

        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);

            if (child is TextBox tb)
            {
                var binding = tb.GetBindingExpression(TextBox.TextProperty);
                var path = binding?.ParentBinding?.Path?.Path;
                if (path is "FilterText" or "SearchText")
                    return tb;
            }

            var result = FindSearchTextBox(child);
            if (result != null) return result;
        }
        return null;
    }

    private T? FindVisualChild<T>(DependencyObject? obj) where T : DependencyObject
    {
        if (obj == null) return null;

        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
            if (child is T match) return match;

            var result = FindVisualChild<T>(child);
            if (result != null) return result;
        }
        return null;
    }
}

public class ActionCommand : ICommand
{
    private readonly Action _execute;
    public ActionCommand(Action execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}
using Microsoft.EntityFrameworkCore;
using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Company;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Entities.Items;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Enums;

namespace QBD.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly QBDesktopDbContext _context;

    public DatabaseSeeder(QBDesktopDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Accounts.AnyAsync())
            return; // Already seeded

        await SeedCompanyInfoAsync();
        await SeedChartOfAccountsAsync();
        await SeedTermsAsync();
        await SeedPaymentMethodsAsync();
        await SeedTaxCodesAsync();
        await SeedFiscalYearAsync();
        await SeedNumberSequencesAsync();
        await SeedCustomersAsync();
        await SeedVendorsAsync();
        await SeedItemsAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedCompanyInfoAsync()
    {
        _context.CompanyInfo.Add(new CompanyInfo
        {
            Name = "My Company",
            LegalName = "My Company, LLC",
            Address = "123 Main Street",
            City = "Anytown",
            State = "CA",
            Zip = "90210",
            Phone = "(555) 555-1234",
            Email = "info@mycompany.com",
            FiscalYearStartMonth = 1
        });
        await _context.SaveChangesAsync();
    }

    private async Task SeedChartOfAccountsAsync()
    {
        var accounts = new List<Account>
        {
            // ===== BANK ACCOUNTS (1000s) =====
            new() { Number = "1000", Name = "Checking", AccountType = AccountType.Bank, IsDebitNormal = true, SortOrder = 1, IsSystemAccount = true },
            new() { Number = "1010", Name = "Savings", AccountType = AccountType.Bank, IsDebitNormal = true, SortOrder = 2 },
            new() { Number = "1020", Name = "Petty Cash", AccountType = AccountType.Bank, IsDebitNormal = true, SortOrder = 3 },

            // ===== ACCOUNTS RECEIVABLE (1100s) =====
            new() { Number = "1100", Name = "Accounts Receivable", AccountType = AccountType.AccountsReceivable, IsDebitNormal = true, SortOrder = 10, IsSystemAccount = true },

            // ===== OTHER CURRENT ASSETS (1200s) =====
            new() { Number = "1200", Name = "Undeposited Funds", AccountType = AccountType.OtherCurrentAsset, IsDebitNormal = true, SortOrder = 20, IsSystemAccount = true },
            new() { Number = "1250", Name = "Inventory Asset", AccountType = AccountType.OtherCurrentAsset, IsDebitNormal = true, SortOrder = 21 },
            new() { Number = "1260", Name = "Employee Advances", AccountType = AccountType.OtherCurrentAsset, IsDebitNormal = true, SortOrder = 22 },
            new() { Number = "1270", Name = "Prepaid Expenses", AccountType = AccountType.OtherCurrentAsset, IsDebitNormal = true, SortOrder = 23 },
            new() { Number = "1280", Name = "Notes Receivable", AccountType = AccountType.OtherCurrentAsset, IsDebitNormal = true, SortOrder = 24 },

            // ===== FIXED ASSETS (1500s) =====
            new() { Number = "1500", Name = "Furniture and Equipment", AccountType = AccountType.FixedAsset, IsDebitNormal = true, SortOrder = 30 },
            new() { Number = "1510", Name = "Accumulated Depreciation", AccountType = AccountType.FixedAsset, IsDebitNormal = false, SortOrder = 31 },
            new() { Number = "1520", Name = "Vehicles", AccountType = AccountType.FixedAsset, IsDebitNormal = true, SortOrder = 32 },
            new() { Number = "1530", Name = "Buildings", AccountType = AccountType.FixedAsset, IsDebitNormal = true, SortOrder = 33 },
            new() { Number = "1540", Name = "Land", AccountType = AccountType.FixedAsset, IsDebitNormal = true, SortOrder = 34 },
            new() { Number = "1550", Name = "Leasehold Improvements", AccountType = AccountType.FixedAsset, IsDebitNormal = true, SortOrder = 35 },

            // ===== OTHER ASSETS (1700s) =====
            new() { Number = "1700", Name = "Security Deposits", AccountType = AccountType.OtherAsset, IsDebitNormal = true, SortOrder = 40 },
            new() { Number = "1710", Name = "Organization Costs", AccountType = AccountType.OtherAsset, IsDebitNormal = true, SortOrder = 41 },

            // ===== ACCOUNTS PAYABLE (2000s) =====
            new() { Number = "2000", Name = "Accounts Payable", AccountType = AccountType.AccountsPayable, IsDebitNormal = false, SortOrder = 50, IsSystemAccount = true },

            // ===== CREDIT CARD (2100s) =====
            new() { Number = "2100", Name = "Company Credit Card", AccountType = AccountType.CreditCard, IsDebitNormal = false, SortOrder = 55 },

            // ===== OTHER CURRENT LIABILITIES (2200s) =====
            new() { Number = "2200", Name = "Sales Tax Payable", AccountType = AccountType.OtherCurrentLiability, IsDebitNormal = false, SortOrder = 60, IsSystemAccount = true },
            new() { Number = "2210", Name = "Payroll Liabilities", AccountType = AccountType.OtherCurrentLiability, IsDebitNormal = false, SortOrder = 61 },
            new() { Number = "2220", Name = "Federal Tax Withholding", AccountType = AccountType.OtherCurrentLiability, IsDebitNormal = false, SortOrder = 62 },
            new() { Number = "2230", Name = "State Tax Withholding", AccountType = AccountType.OtherCurrentLiability, IsDebitNormal = false, SortOrder = 63 },
            new() { Number = "2240", Name = "FICA Payable", AccountType = AccountType.OtherCurrentLiability, IsDebitNormal = false, SortOrder = 64 },
            new() { Number = "2250", Name = "Line of Credit", AccountType = AccountType.OtherCurrentLiability, IsDebitNormal = false, SortOrder = 65 },
            new() { Number = "2260", Name = "Accrued Liabilities", AccountType = AccountType.OtherCurrentLiability, IsDebitNormal = false, SortOrder = 66 },

            // ===== LONG-TERM LIABILITIES (2500s) =====
            new() { Number = "2500", Name = "Mortgage Payable", AccountType = AccountType.LongTermLiability, IsDebitNormal = false, SortOrder = 70 },
            new() { Number = "2510", Name = "Notes Payable", AccountType = AccountType.LongTermLiability, IsDebitNormal = false, SortOrder = 71 },
            new() { Number = "2520", Name = "Vehicle Loan", AccountType = AccountType.LongTermLiability, IsDebitNormal = false, SortOrder = 72 },

            // ===== EQUITY (3000s) =====
            new() { Number = "3000", Name = "Opening Balance Equity", AccountType = AccountType.Equity, IsDebitNormal = false, SortOrder = 80, IsSystemAccount = true },
            new() { Number = "3100", Name = "Owner's Equity", AccountType = AccountType.Equity, IsDebitNormal = false, SortOrder = 81 },
            new() { Number = "3200", Name = "Owner's Draw", AccountType = AccountType.Equity, IsDebitNormal = true, SortOrder = 82 },
            new() { Number = "3300", Name = "Retained Earnings", AccountType = AccountType.Equity, IsDebitNormal = false, SortOrder = 83, IsSystemAccount = true },
            new() { Number = "3400", Name = "Common Stock", AccountType = AccountType.Equity, IsDebitNormal = false, SortOrder = 84 },
            new() { Number = "3500", Name = "Paid-in Capital", AccountType = AccountType.Equity, IsDebitNormal = false, SortOrder = 85 },

            // ===== INCOME (4000s) =====
            new() { Number = "4000", Name = "Sales", AccountType = AccountType.Income, IsDebitNormal = false, SortOrder = 90 },
            new() { Number = "4010", Name = "Services", AccountType = AccountType.Income, IsDebitNormal = false, SortOrder = 91 },
            new() { Number = "4020", Name = "Discounts Given", AccountType = AccountType.Income, IsDebitNormal = true, SortOrder = 92 },
            new() { Number = "4030", Name = "Shipping & Delivery Income", AccountType = AccountType.Income, IsDebitNormal = false, SortOrder = 93 },
            new() { Number = "4040", Name = "Sales Returns & Allowances", AccountType = AccountType.Income, IsDebitNormal = true, SortOrder = 94 },

            // ===== OTHER INCOME (4500s) =====
            new() { Number = "4500", Name = "Interest Income", AccountType = AccountType.OtherIncome, IsDebitNormal = false, SortOrder = 100 },
            new() { Number = "4510", Name = "Other Income", AccountType = AccountType.OtherIncome, IsDebitNormal = false, SortOrder = 101 },
            new() { Number = "4520", Name = "Gain on Sale of Assets", AccountType = AccountType.OtherIncome, IsDebitNormal = false, SortOrder = 102 },

            // ===== COST OF GOODS SOLD (5000s) =====
            new() { Number = "5000", Name = "Cost of Goods Sold", AccountType = AccountType.CostOfGoodsSold, IsDebitNormal = true, SortOrder = 110 },
            new() { Number = "5010", Name = "Purchases", AccountType = AccountType.CostOfGoodsSold, IsDebitNormal = true, SortOrder = 111 },
            new() { Number = "5020", Name = "Freight & Delivery - COGS", AccountType = AccountType.CostOfGoodsSold, IsDebitNormal = true, SortOrder = 112 },
            new() { Number = "5030", Name = "Subcontractors - COGS", AccountType = AccountType.CostOfGoodsSold, IsDebitNormal = true, SortOrder = 113 },
            new() { Number = "5040", Name = "Materials - COGS", AccountType = AccountType.CostOfGoodsSold, IsDebitNormal = true, SortOrder = 114 },

            // ===== EXPENSES (6000s-6900s) =====
            new() { Number = "6000", Name = "Advertising & Promotion", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 120 },
            new() { Number = "6010", Name = "Auto Expense", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 121 },
            new() { Number = "6020", Name = "Bank Service Charges", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 122 },
            new() { Number = "6030", Name = "Depreciation Expense", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 123 },
            new() { Number = "6040", Name = "Dues & Subscriptions", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 124 },
            new() { Number = "6050", Name = "Equipment Rental", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 125 },
            new() { Number = "6060", Name = "Insurance", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 126 },
            new() { Number = "6100", Name = "Interest Expense", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 127 },
            new() { Number = "6110", Name = "Legal & Professional Fees", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 128 },
            new() { Number = "6120", Name = "Meals & Entertainment", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 129 },
            new() { Number = "6130", Name = "Office Supplies", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 130 },
            new() { Number = "6140", Name = "Payroll Expenses", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 131 },
            new() { Number = "6150", Name = "Postage & Delivery", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 132 },
            new() { Number = "6160", Name = "Printing & Reproduction", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 133 },
            new() { Number = "6170", Name = "Professional Development", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 134 },
            new() { Number = "6180", Name = "Rent", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 135 },
            new() { Number = "6190", Name = "Repairs & Maintenance", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 136 },
            new() { Number = "6200", Name = "Taxes - Property", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 137 },
            new() { Number = "6210", Name = "Telephone", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 138 },
            new() { Number = "6220", Name = "Travel", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 139 },
            new() { Number = "6230", Name = "Utilities", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 140 },
            new() { Number = "6240", Name = "Miscellaneous", AccountType = AccountType.Expense, IsDebitNormal = true, SortOrder = 141 },

            // ===== OTHER EXPENSE (7000s) =====
            new() { Number = "7000", Name = "Ask My Accountant", AccountType = AccountType.OtherExpense, IsDebitNormal = true, SortOrder = 150 },
            new() { Number = "7010", Name = "Penalties & Fines", AccountType = AccountType.OtherExpense, IsDebitNormal = true, SortOrder = 151 },
            new() { Number = "7020", Name = "Loss on Sale of Assets", AccountType = AccountType.OtherExpense, IsDebitNormal = true, SortOrder = 152 },
        };

        await _context.Accounts.AddRangeAsync(accounts);
        await _context.SaveChangesAsync();
    }

    private async Task SeedTermsAsync()
    {
        var terms = new List<Terms>
        {
            new() { Name = "Due on Receipt", DueDays = 0 },
            new() { Name = "Net 15", DueDays = 15 },
            new() { Name = "Net 30", DueDays = 30 },
            new() { Name = "Net 60", DueDays = 60 },
            new() { Name = "2/10 Net 30", DueDays = 30, DiscountDays = 10, DiscountPercent = 2m },
        };
        await _context.Terms.AddRangeAsync(terms);
        await _context.SaveChangesAsync();
    }

    private async Task SeedPaymentMethodsAsync()
    {
        var methods = new List<PaymentMethod>
        {
            new() { Name = "Check" },
            new() { Name = "Cash" },
            new() { Name = "Credit Card" },
            new() { Name = "Bank Transfer" },
        };
        await _context.PaymentMethods.AddRangeAsync(methods);
        await _context.SaveChangesAsync();
    }

    private async Task SeedTaxCodesAsync()
    {
        var codes = new List<TaxCode>
        {
            new() { Name = "Tax", Rate = 8.25m },
            new() { Name = "Non", Rate = 0 },
        };
        await _context.TaxCodes.AddRangeAsync(codes);
        await _context.SaveChangesAsync();
    }

    private async Task SeedFiscalYearAsync()
    {
        var year = new FiscalYear
        {
            Name = "FY 2026",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
        };
        _context.FiscalYears.Add(year);
        await _context.SaveChangesAsync();

        // 12 regular periods + 1 adjusting
        for (int i = 1; i <= 12; i++)
        {
            _context.FiscalPeriods.Add(new FiscalPeriod
            {
                FiscalYearId = year.Id,
                PeriodNumber = i,
                Name = new DateTime(2026, i, 1).ToString("MMMM yyyy"),
                StartDate = new DateTime(2026, i, 1),
                EndDate = new DateTime(2026, i, DateTime.DaysInMonth(2026, i)),
            });
        }
        // Period 13 - Adjusting
        _context.FiscalPeriods.Add(new FiscalPeriod
        {
            FiscalYearId = year.Id,
            PeriodNumber = 13,
            Name = "Adjusting Period 2026",
            StartDate = new DateTime(2026, 12, 31),
            EndDate = new DateTime(2026, 12, 31),
            IsAdjusting = true
        });
        await _context.SaveChangesAsync();
    }

    private async Task SeedNumberSequencesAsync()
    {
        var sequences = new List<NumberSequence>
        {
            new() { EntityType = "Invoice", Prefix = "INV-", NextNumber = 1001 },
            new() { EntityType = "Estimate", Prefix = "EST-", NextNumber = 1001 },
            new() { EntityType = "SalesReceipt", Prefix = "SR-", NextNumber = 1001 },
            new() { EntityType = "CreditMemo", Prefix = "CM-", NextNumber = 1001 },
            new() { EntityType = "Bill", Prefix = "BILL-", NextNumber = 1001 },
            new() { EntityType = "PurchaseOrder", Prefix = "PO-", NextNumber = 1001 },
            new() { EntityType = "Check", Prefix = "CHK-", NextNumber = 1001 },
            new() { EntityType = "JournalEntry", Prefix = "JE-", NextNumber = 1001 },
            new() { EntityType = "VendorCredit", Prefix = "VC-", NextNumber = 1001 },
        };
        await _context.NumberSequences.AddRangeAsync(sequences);
        await _context.SaveChangesAsync();
    }

    private async Task SeedCustomersAsync()
    {
        var net30 = await _context.Terms.FirstAsync(t => t.Name == "Net 30");
        var customers = new List<Customer>
        {
            new() { CustomerName = "Acme Corporation", Company = "Acme Corp", BillToAddress = "100 Industrial Pkwy\nSan Jose, CA 95110", Phone = "(408) 555-0100", Email = "ap@acmecorp.com", TermsId = net30.Id, IsActive = true },
            new() { CustomerName = "Baker Industries", Company = "Baker Industries LLC", BillToAddress = "200 Commerce Dr\nLos Angeles, CA 90001", Phone = "(213) 555-0200", Email = "billing@baker.com", TermsId = net30.Id, IsActive = true },
            new() { CustomerName = "Carter & Associates", Company = "Carter & Associates", BillToAddress = "300 Professional Way\nSan Francisco, CA 94102", Phone = "(415) 555-0300", Email = "info@carter.com", TermsId = net30.Id, IsActive = true },
            new() { CustomerName = "Davis Construction", Company = "Davis Construction Inc", BillToAddress = "400 Builder Ln\nSacramento, CA 95814", Phone = "(916) 555-0400", Email = "office@davisconstruction.com", TermsId = net30.Id, IsActive = true },
            new() { CustomerName = "Evans Medical Group", Company = "Evans Medical", BillToAddress = "500 Health Ave\nSan Diego, CA 92101", Phone = "(619) 555-0500", Email = "billing@evansmedical.com", TermsId = net30.Id, IsActive = true },
            new() { CustomerName = "Foster Electronics", Company = "Foster Electronics", BillToAddress = "600 Tech Blvd\nSan Jose, CA 95112", Phone = "(408) 555-0600", Email = "purchasing@fosterelec.com", TermsId = net30.Id, IsActive = true },
            new() { CustomerName = "Green Landscaping", Company = "Green Landscaping Co", BillToAddress = "700 Garden Rd\nFresno, CA 93721", Phone = "(559) 555-0700", Email = "office@greenlandscaping.com", TermsId = net30.Id, IsActive = true },
            new() { CustomerName = "Harris & Sons", Company = "Harris & Sons Inc", BillToAddress = "800 Family St\nOakland, CA 94601", Phone = "(510) 555-0800", Email = "contact@harrissons.com", TermsId = net30.Id, IsActive = true },
            new() { CustomerName = "Irving Technologies", Company = "Irving Tech", BillToAddress = "900 Innovation Ct\nPalo Alto, CA 94301", Phone = "(650) 555-0900", Email = "ar@irvingtech.com", TermsId = net30.Id, IsActive = true },
            new() { CustomerName = "Jackson Consulting", Company = "Jackson Consulting LLC", BillToAddress = "1000 Advisory Pl\nBeverly Hills, CA 90210", Phone = "(310) 555-1000", Email = "info@jacksonconsulting.com", TermsId = net30.Id, IsActive = true },
        };
        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();
    }

    private async Task SeedVendorsAsync()
    {
        var net30 = await _context.Terms.FirstAsync(t => t.Name == "Net 30");
        var vendors = new List<Vendor>
        {
            new() { VendorName = "Office Depot", Company = "Office Depot Inc", Address = "1 Supply Way\nAustin, TX 78701", Phone = "(800) 555-3000", TermsId = net30.Id, IsActive = true },
            new() { VendorName = "Pacific Gas & Electric", Company = "PG&E", Address = "2 Utility Ave\nSan Francisco, CA 94105", Phone = "(800) 555-3001", TermsId = net30.Id, IsActive = true },
            new() { VendorName = "AT&T", Company = "AT&T Inc", Address = "3 Telecom Blvd\nDallas, TX 75201", Phone = "(800) 555-3002", TermsId = net30.Id, IsActive = true },
            new() { VendorName = "State Farm Insurance", Company = "State Farm", Address = "4 Insurance Ln\nBloomington, IL 61701", Phone = "(800) 555-3003", TermsId = net30.Id, IsActive = true },
            new() { VendorName = "Dell Technologies", Company = "Dell Inc", Address = "5 Computer Way\nRound Rock, TX 78664", Phone = "(800) 555-3004", TermsId = net30.Id, IsActive = true },
            new() { VendorName = "Amazon Business", Company = "Amazon.com Inc", Address = "6 Commerce St\nSeattle, WA 98101", Phone = "(800) 555-3005", TermsId = net30.Id, IsActive = true },
            new() { VendorName = "UPS", Company = "United Parcel Service", Address = "7 Shipping Rd\nAtlanta, GA 30301", Phone = "(800) 555-3006", TermsId = net30.Id, IsActive = true },
            new() { VendorName = "Staples", Company = "Staples Inc", Address = "8 Office Park\nFramingham, MA 01701", Phone = "(800) 555-3007", TermsId = net30.Id, IsActive = true },
        };
        await _context.Vendors.AddRangeAsync(vendors);
        await _context.SaveChangesAsync();
    }

    private async Task SeedItemsAsync()
    {
        var salesAccount = await _context.Accounts.FirstAsync(a => a.Number == "4000");
        var servicesAccount = await _context.Accounts.FirstAsync(a => a.Number == "4010");
        var cogsAccount = await _context.Accounts.FirstAsync(a => a.Number == "5000");
        var inventoryAsset = await _context.Accounts.FirstAsync(a => a.Number == "1250");

        var items = new List<Item>
        {
            // Service items
            new() { ItemName = "Consulting", ItemType = ItemType.Service, Description = "Professional consulting services", SalesPrice = 150m, IncomeAccountId = servicesAccount.Id, IsActive = true },
            new() { ItemName = "Installation", ItemType = ItemType.Service, Description = "Installation services", SalesPrice = 85m, IncomeAccountId = servicesAccount.Id, IsActive = true },
            new() { ItemName = "Design Services", ItemType = ItemType.Service, Description = "Design and creative services", SalesPrice = 125m, IncomeAccountId = servicesAccount.Id, IsActive = true },
            new() { ItemName = "Training", ItemType = ItemType.Service, Description = "Training and education services", SalesPrice = 200m, IncomeAccountId = servicesAccount.Id, IsActive = true },
            new() { ItemName = "Support Contract", ItemType = ItemType.Service, Description = "Annual support contract", SalesPrice = 500m, IncomeAccountId = servicesAccount.Id, IsActive = true },

            // Inventory items
            new() { ItemName = "Widget A", ItemType = ItemType.InventoryPart, Description = "Standard Widget A", SalesPrice = 25m, PurchaseCost = 12m, IncomeAccountId = salesAccount.Id, ExpenseAccountId = cogsAccount.Id, AssetAccountId = inventoryAsset.Id, QtyOnHand = 100, ReorderPoint = 20, IsActive = true },
            new() { ItemName = "Widget B", ItemType = ItemType.InventoryPart, Description = "Premium Widget B", SalesPrice = 45m, PurchaseCost = 22m, IncomeAccountId = salesAccount.Id, ExpenseAccountId = cogsAccount.Id, AssetAccountId = inventoryAsset.Id, QtyOnHand = 75, ReorderPoint = 15, IsActive = true },
            new() { ItemName = "Gadget Pro", ItemType = ItemType.InventoryPart, Description = "Gadget Pro model", SalesPrice = 199m, PurchaseCost = 95m, IncomeAccountId = salesAccount.Id, ExpenseAccountId = cogsAccount.Id, AssetAccountId = inventoryAsset.Id, QtyOnHand = 30, ReorderPoint = 10, IsActive = true },

            // Non-inventory items
            new() { ItemName = "Shipping", ItemType = ItemType.NonInventoryPart, Description = "Shipping and handling", SalesPrice = 15m, IncomeAccountId = salesAccount.Id, IsActive = true },
            new() { ItemName = "Gift Wrap", ItemType = ItemType.NonInventoryPart, Description = "Gift wrapping service", SalesPrice = 5m, IncomeAccountId = salesAccount.Id, IsActive = true },

            // Other
            new() { ItemName = "Hourly Labor", ItemType = ItemType.OtherCharge, Description = "Hourly labor charge", SalesPrice = 75m, IncomeAccountId = servicesAccount.Id, IsActive = true },
            new() { ItemName = "Volume Discount", ItemType = ItemType.Discount, Description = "10% volume discount", SalesPrice = 0m, IncomeAccountId = salesAccount.Id, IsActive = true },
            new() { ItemName = "Subtotal", ItemType = ItemType.Subtotal, Description = "Subtotal", IsActive = true },
            new() { ItemName = "CA Sales Tax", ItemType = ItemType.SalesTaxItem, Description = "California Sales Tax 8.25%", SalesPrice = 8.25m, IsActive = true },
            new() { ItemName = "Misc. Supplies", ItemType = ItemType.NonInventoryPart, Description = "Miscellaneous supplies", SalesPrice = 0m, PurchaseCost = 0m, IncomeAccountId = salesAccount.Id, ExpenseAccountId = cogsAccount.Id, IsActive = true },
        };

        await _context.Items.AddRangeAsync(items);
        await _context.SaveChangesAsync();
    }
}

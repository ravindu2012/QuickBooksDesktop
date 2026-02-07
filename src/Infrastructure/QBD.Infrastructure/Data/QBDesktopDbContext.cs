using Microsoft.EntityFrameworkCore;
using QBD.Domain.Common;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Banking;
using QBD.Domain.Entities.Company;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Entities.Items;
using QBD.Domain.Entities.Vendors;

namespace QBD.Infrastructure.Data;

public class QBDesktopDbContext : DbContext
{
    public QBDesktopDbContext(DbContextOptions<QBDesktopDbContext> options) : base(options) { }

    // Company
    public DbSet<CompanyInfo> CompanyInfo => Set<CompanyInfo>();
    public DbSet<Preference> Preferences => Set<Preference>();
    public DbSet<User> Users => Set<User>();

    // Accounting
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<GLEntry> GLEntries => Set<GLEntry>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Terms> Terms => Set<Terms>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<TaxCode> TaxCodes => Set<TaxCode>();

    // Common
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    // Customers
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Estimate> Estimates => Set<Estimate>();
    public DbSet<EstimateLine> EstimateLines => Set<EstimateLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<ReceivePayment> ReceivePayments => Set<ReceivePayment>();
    public DbSet<PaymentApplication> PaymentApplications => Set<PaymentApplication>();
    public DbSet<SalesReceipt> SalesReceipts => Set<SalesReceipt>();
    public DbSet<SalesReceiptLine> SalesReceiptLines => Set<SalesReceiptLine>();
    public DbSet<CreditMemo> CreditMemos => Set<CreditMemo>();
    public DbSet<CreditMemoLine> CreditMemoLines => Set<CreditMemoLine>();

    // Vendors
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillExpenseLine> BillExpenseLines => Set<BillExpenseLine>();
    public DbSet<BillItemLine> BillItemLines => Set<BillItemLine>();
    public DbSet<BillPayment> BillPayments => Set<BillPayment>();
    public DbSet<BillPaymentApplication> BillPaymentApplications => Set<BillPaymentApplication>();
    public DbSet<VendorCredit> VendorCredits => Set<VendorCredit>();
    public DbSet<VendorCreditLine> VendorCreditLines => Set<VendorCreditLine>();

    // Banking
    public DbSet<Check> Checks => Set<Check>();
    public DbSet<CheckExpenseLine> CheckExpenseLines => Set<CheckExpenseLine>();
    public DbSet<CheckItemLine> CheckItemLines => Set<CheckItemLine>();
    public DbSet<Deposit> Deposits => Set<Deposit>();
    public DbSet<DepositLine> DepositLines => Set<DepositLine>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<Reconciliation> Reconciliations => Set<Reconciliation>();
    public DbSet<ReconciliationLine> ReconciliationLines => Set<ReconciliationLine>();

    // Items
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply soft-delete global filter to all BaseEntity derivatives
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(QBDesktopDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        // Decimal precision for money fields
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // Account self-referencing
        modelBuilder.Entity<Account>(e =>
        {
            e.HasOne(a => a.Parent)
                .WithMany(a => a.SubAccounts)
                .HasForeignKey(a => a.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(a => a.Number).IsUnique().HasFilter("Number IS NOT NULL");
            e.HasIndex(a => a.Name);
        });

        // Class self-referencing
        modelBuilder.Entity<Class>(e =>
        {
            e.HasOne(c => c.Parent)
                .WithMany(c => c.SubClasses)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Location self-referencing
        modelBuilder.Entity<Location>(e =>
        {
            e.HasOne(l => l.Parent)
                .WithMany(l => l.SubLocations)
                .HasForeignKey(l => l.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Item self-referencing
        modelBuilder.Entity<Item>(e =>
        {
            e.HasOne(i => i.ParentItem)
                .WithMany(i => i.SubItems)
                .HasForeignKey(i => i.ParentItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // GL Entry indexes
        modelBuilder.Entity<GLEntry>(e =>
        {
            e.HasIndex(g => new { g.AccountId, g.PostingDate });
            e.HasIndex(g => new { g.TransactionType, g.TransactionId });
        });

        // Invoice
        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasIndex(i => i.InvoiceNumber).IsUnique();
        });

        // Bill
        modelBuilder.Entity<Bill>(e =>
        {
            e.HasOne(b => b.Vendor)
                .WithMany(v => v.Bills)
                .HasForeignKey(b => b.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Transfer - multiple FK to Account
        modelBuilder.Entity<Transfer>(e =>
        {
            e.HasOne(t => t.FromAccount)
                .WithMany()
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.ToAccount)
                .WithMany()
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Check
        modelBuilder.Entity<Check>(e =>
        {
            e.HasOne(c => c.BankAccount)
                .WithMany()
                .HasForeignKey(c => c.BankAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Deposit
        modelBuilder.Entity<Deposit>(e =>
        {
            e.HasOne(d => d.BankAccount)
                .WithMany()
                .HasForeignKey(d => d.BankAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BillPayment
        modelBuilder.Entity<BillPayment>(e =>
        {
            e.HasOne(bp => bp.PaymentAccount)
                .WithMany()
                .HasForeignKey(bp => bp.PaymentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BillPaymentApplication
        modelBuilder.Entity<BillPaymentApplication>(e =>
        {
            e.HasOne(bpa => bpa.BillPayment)
                .WithMany(bp => bp.Applications)
                .HasForeignKey(bpa => bpa.BillPaymentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(bpa => bpa.Bill)
                .WithMany()
                .HasForeignKey(bpa => bpa.BillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ReceivePayment
        modelBuilder.Entity<ReceivePayment>(e =>
        {
            e.HasOne(rp => rp.DepositToAccount)
                .WithMany()
                .HasForeignKey(rp => rp.DepositToAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PaymentApplication
        modelBuilder.Entity<PaymentApplication>(e =>
        {
            e.HasOne(pa => pa.ReceivePayment)
                .WithMany(rp => rp.Applications)
                .HasForeignKey(pa => pa.ReceivePaymentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(pa => pa.Invoice)
                .WithMany()
                .HasForeignKey(pa => pa.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Reconciliation
        modelBuilder.Entity<Reconciliation>(e =>
        {
            e.HasOne(r => r.Account)
                .WithMany()
                .HasForeignKey(r => r.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BillExpenseLine - multiple FKs
        modelBuilder.Entity<BillExpenseLine>(e =>
        {
            e.HasOne(bel => bel.Account)
                .WithMany()
                .HasForeignKey(bel => bel.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(bel => bel.Customer)
                .WithMany()
                .HasForeignKey(bel => bel.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BillItemLine
        modelBuilder.Entity<BillItemLine>(e =>
        {
            e.HasOne(bil => bil.Customer)
                .WithMany()
                .HasForeignKey(bil => bil.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CheckExpenseLine
        modelBuilder.Entity<CheckExpenseLine>(e =>
        {
            e.HasOne(cel => cel.Account)
                .WithMany()
                .HasForeignKey(cel => cel.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(cel => cel.Customer)
                .WithMany()
                .HasForeignKey(cel => cel.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DepositLine
        modelBuilder.Entity<DepositLine>(e =>
        {
            e.HasOne(dl => dl.FromAccount)
                .WithMany()
                .HasForeignKey(dl => dl.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // NumberSequence
        modelBuilder.Entity<NumberSequence>(e =>
        {
            e.HasIndex(ns => ns.EntityType).IsUnique();
        });

        // SalesReceipt
        modelBuilder.Entity<SalesReceipt>(e =>
        {
            e.HasOne(sr => sr.DepositToAccount)
                .WithMany()
                .HasForeignKey(sr => sr.DepositToAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Item accounts
        modelBuilder.Entity<Item>(e =>
        {
            e.HasOne(i => i.IncomeAccount)
                .WithMany()
                .HasForeignKey(i => i.IncomeAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(i => i.ExpenseAccount)
                .WithMany()
                .HasForeignKey(i => i.ExpenseAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(i => i.AssetAccount)
                .WithMany()
                .HasForeignKey(i => i.AssetAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}

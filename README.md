<div align="center">

# QuickBooks Desktop Enterprise Clone

**A full-featured desktop accounting application built with WPF and .NET 8**

Inspired by QuickBooks Desktop Enterprise. Built for small and medium businesses.

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com)
[![WPF](https://img.shields.io/badge/WPF-Desktop-blue)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
[![Status](https://img.shields.io/badge/Status-In%20Development-orange)]()

</div>

---

> **This project is under active development.** Features are being added regularly. Contributions, feedback, and ideas are welcome!

---

## What Is This?

A desktop accounting application modeled after QuickBooks Desktop Enterprise, built from scratch with modern .NET technologies and clean architecture. It provides double-entry bookkeeping, customer/vendor management, invoicing, bill payments, banking, and financial reporting.

---

## Features

### Accounting
- Double-entry accounting engine with 11 transaction posting types
- 65+ seeded Chart of Accounts
- General Ledger with balanced debits and credits
- Journal entries with reversing/void support
- Fiscal year and period management
- Class and location tracking

### Customers
- Customer Center with 3-panel layout
- Invoices, Estimates, Sales Receipts
- Receive Payments with application to open invoices
- Credit Memos
- AR Aging reports

### Vendors
- Vendor Center with 3-panel layout
- Bills, Purchase Orders, Vendor Credits
- Bill Payments with application to open bills
- AP Aging reports

### Banking
- Write Checks, Make Deposits, Transfer Funds
- Bank Register view
- Bank Reconciliation

### Reports
- Profit & Loss
- Balance Sheet
- Cash Flows
- Trial Balance
- General Ledger
- AR/AP Aging (Summary & Detail)
- Customer & Vendor Balance
- Open Invoices & Unpaid Bills
- Transaction List
- Deposit Detail

### Application
- Classic QuickBooks-style UI with menu bar and MDI tabs
- Sortable and filterable lists
- Soft delete (no data is permanently removed)
- Auto-generated transaction numbers
- Database auto-seeding with sample data

---

## Architecture

Clean Architecture with 4 layers. Dependencies flow inward only:

```
Domain (entities, enums)
  ← Application (interfaces, base ViewModels, DTOs)
    ← Infrastructure (EF Core, repositories, services)
      ← Presentation (WPF shell + 7 module projects)
```

**13 projects** organized as:

| Layer | Projects |
|-------|----------|
| Domain | `QBD.Domain` |
| Application | `QBD.Application` |
| Infrastructure | `QBD.Infrastructure` |
| Presentation | `QBD.WPF` + 7 module projects (`Company`, `Customers`, `Vendors`, `Banking`, `Reports`, `Employees`, `Inventory`) |
| Tests | `QBD.Domain.Tests`, `QBD.IntegrationTests` |

---

## Tech Stack

- **Framework**: .NET 8 + WPF
- **MVVM**: CommunityToolkit.Mvvm 8.2.2 (source generators)
- **ORM**: Entity Framework Core 8.0.11
- **Database**: SQL Server LocalDB
- **Architecture**: Clean Architecture + MVVM

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (included with Visual Studio)

### Build & Run

```bash
# Build the solution
dotnet build QuickBooksDesktop.slnx

# Run the application
dotnet run --project src/Presentation/QBD.WPF/QBD.WPF.csproj

# Run tests
dotnet test QuickBooksDesktop.slnx
```

The database is auto-created on first run with sample data (customers, vendors, items, chart of accounts, etc.).

---

## Roadmap

- [ ] Employees module
- [ ] Inventory module
- [ ] Print/export invoices and reports to PDF
- [ ] Multi-currency support
- [ ] User roles and permissions
- [ ] Data import/export

---

## Contributing

This project is in active development and contributions are welcome! Here's how you can help:

### Ways to Contribute

- **Report bugs** — Open an [issue](https://github.com/ravindu2012/QuickBooksDesktop/issues) with steps to reproduce
- **Suggest features** — Share your ideas in [Discussions](https://github.com/ravindu2012/QuickBooksDesktop/discussions) or open an issue
- **Submit code** — Pick an open issue or propose a change via pull request
- **Improve docs** — Fix typos, add examples, or improve the README
- **Test the app** — Try it out and report what works and what doesn't

### How to Submit a Pull Request

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Make your changes
4. Run the tests (`dotnet test QuickBooksDesktop.slnx`)
5. Commit your changes (`git commit -m "Add your feature"`)
6. Push to your fork (`git push origin feature/your-feature`)
7. Open a Pull Request

### Good First Issues

Look for issues labeled [`good first issue`](https://github.com/ravindu2012/QuickBooksDesktop/labels/good%20first%20issue) — these are great starting points for new contributors.

---

## License

This project is open source. See [LICENSE](LICENSE) for details.

---

<div align="center">

**A [Raveforge](https://github.com/ravindu2012) Product**

If you find this useful, consider [sponsoring](https://github.com/sponsors/ravindu2012) the project.

</div>

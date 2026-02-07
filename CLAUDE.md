# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build entire solution (13 projects)
dotnet build QuickBooksDesktop.slnx

# Run the WPF application
dotnet run --project src/Presentation/QBD.WPF/QBD.WPF.csproj

# Run all tests
dotnet test QuickBooksDesktop.slnx

# Run a single test project
dotnet test tests/QBD.Domain.Tests/QBD.Domain.Tests.csproj
dotnet test tests/QBD.IntegrationTests/QBD.IntegrationTests.csproj
```

**Important**: The solution file is `QuickBooksDesktop.slnx` (new .NET SDK format), not `.sln`.

## Database

SQL Server LocalDB with EF Core 8.0.11. Connection string is hard-coded in `App.xaml.cs`:
```
Server=(localdb)\MSSQLLocalDB;Database=QuickBooksDesktop;Trusted_Connection=true;TrustServerCertificate=true;
```
Database is auto-created via `EnsureCreatedAsync` on startup (no migrations). `DatabaseSeeder` populates ~65 chart of accounts, sample customers/vendors/items, fiscal year, terms, and payment methods.

## Architecture

Clean Architecture with 4 layers. Dependencies flow inward only:

```
Domain (entities, enums) ← Application (interfaces, base VMs, DTOs)
    ← Infrastructure (EF Core, repositories, services)
    ← Presentation (WPF shell + 7 module projects)
```

**Key constraint**: Module projects (`QBD.Modules.*`) reference only `QBD.Application` — never Infrastructure or other modules. All concrete services are injected via DI configured in `App.xaml.cs`.

### MVVM Pattern

Uses CommunityToolkit.Mvvm 8.2.2 **source generators** (`[ObservableProperty]`, `[RelayCommand]`, `partial` classes). Base ViewModels live in `QBD.Application/ViewModels/` (not the WPF project) to avoid circular references.

ViewModel hierarchy:
- `ViewModelBase` → all VMs inherit from this
- `ListViewModelBase<TDto>` → sortable/filterable lists (COA, Items, Terms, etc.)
- `CenterViewModelBase<TListDto, TFormDto>` → 3-panel centers (Customer Center, Vendor Center)
- `TransactionFormViewModelBase<TEntity, TLineEntity>` → header + line items forms (Invoice, Bill, Check, etc.)
- `RegisterViewModelBase` → bank register view
- `ReportViewModelBase` → all 16 report VMs

### View Resolution

No individual XAML views per ViewModel. `ViewModelTemplateSelector` in `WPF/Controls/` uses reflection to detect the base generic type of each VM and maps it to a DataTemplate defined in `WPF/Themes/DataTemplates.xaml`. Templates: HomePageTemplate, CenterTemplate, TransactionFormTemplate, RegisterTemplate, ListTemplate, ReportTemplate, DefaultTemplate.

### Navigation

String-based routing via `NavigationService` (`WPF/Services/`). `MainWindow` manages an MDI-style `TabControl` workspace. Navigation methods: `OpenForm("Invoice")`, `OpenCenter("CustomerCenter")`, `OpenList("ChartOfAccounts")`, `OpenReport("ProfitLoss")`, `OpenRegister(accountId)`.

### Double-Entry Accounting Engine

`TransactionPostingService` (`Infrastructure/Services/`) creates balanced GL entries for 11 transaction types (Invoice, Payment, Bill, Check, Deposit, Transfer, etc.). Every posted transaction must have equal debits and credits. Void creates reversing entries.

### Soft Delete

`BaseEntity` has `IsDeleted` flag. `QBDesktopDbContext` applies a global query filter and intercepts `EntityState.Deleted` in `SaveChangesAsync` to set `IsDeleted = true` instead of hard-deleting. All decimal properties are auto-configured to precision(18, 2).

## Common Gotchas

- **Namespace collision**: `QBD.Application` conflicts with `System.Windows.Application` in WPF project. Use fully qualified `System.Windows.Application` in `App.xaml.cs` and `MainWindow.xaml.cs`.
- **Module project references**: Path from `src/Presentation/Modules/<Module>/` to `src/Core/` requires exactly 3 `..` segments: `..\..\..\Core\QBD.Application\QBD.Application.csproj`.
- **DI registration**: Every new ViewModel must be registered as `Transient` in `App.xaml.cs` and its navigation key added to `NavigationService.cs`.
- **Employees/Inventory modules**: Phase 2 placeholders — empty projects with no ViewModels.

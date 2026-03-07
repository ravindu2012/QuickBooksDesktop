# Contributing to QuickBooks Desktop Clone

Thank you for your interest in contributing! This guide will help you get started.

## How to Contribute

### Reporting Bugs
1. Check [existing issues](https://github.com/ravindu2012/QuickBooksDesktop/issues) to avoid duplicates
2. Open a new issue using the **Bug Report** template
3. Include steps to reproduce, expected behavior, and screenshots if possible

### Suggesting Features
1. Open an issue using the **Feature Request** template
2. Describe the use case and how it benefits the project

### Submitting Code
1. **Fork** the repository
2. **Clone** your fork locally
3. **Create a branch** from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```
4. **Make your changes** following the guidelines below
5. **Test** your changes
6. **Commit** with a clear message
7. **Push** to your fork and open a **Pull Request**

### Finding Issues to Work On
- Look for [`good first issue`](https://github.com/ravindu2012/QuickBooksDesktop/labels/good%20first%20issue) labels
- Issues labeled [`help wanted`](https://github.com/ravindu2012/QuickBooksDesktop/labels/help%20wanted) need contributors

## Development Setup

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 (with WPF workload) or JetBrains Rider
- SQL Server LocalDB or SQLite

### Build & Run
```bash
git clone https://github.com/YOUR_USERNAME/QuickBooksDesktop.git
cd QuickBooksDesktop
dotnet restore
dotnet build
dotnet run --project src/Presentation
```

## Code Guidelines

### Architecture
This project follows **Clean Architecture** with 4 layers:
- `Core` — Domain entities, interfaces, business logic
- `Infrastructure` — Data access, EF Core, repositories
- `Presentation` — WPF UI, ViewModels (MVVM pattern)

### Rules
- Dependencies flow inward only (Presentation → Infrastructure → Core)
- Keep ViewModels thin — business logic belongs in Core services
- Follow existing naming conventions and code style
- Add unit tests for new business logic in `tests/`
- Use MVVM pattern — no code-behind logic in views

### Commit Messages
- `Add: description` — new features
- `Fix: description` — bug fixes
- `Update: description` — improvements
- `Refactor: description` — code restructuring
- `Docs: description` — documentation

## Need Help?
- Open a [Discussion](https://github.com/ravindu2012/QuickBooksDesktop/discussions) for questions
- Check the [README](README.md) for project overview

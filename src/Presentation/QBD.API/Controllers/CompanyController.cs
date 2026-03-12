// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Entities.Company;

namespace QBD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly IRepository<CompanyInfo> _companyRepo;
    private readonly IRepository<Terms> _termsRepo;
    private readonly IRepository<PaymentMethod> _paymentMethodRepo;
    private readonly IRepository<TaxCode> _taxCodeRepo;
    private readonly IRepository<Class> _classRepo;
    private readonly IRepository<Location> _locationRepo;
    private readonly IUnitOfWork _uow;

    public CompanyController(
        IRepository<CompanyInfo> companyRepo,
        IRepository<Terms> termsRepo,
        IRepository<PaymentMethod> paymentMethodRepo,
        IRepository<TaxCode> taxCodeRepo,
        IRepository<Class> classRepo,
        IRepository<Location> locationRepo,
        IUnitOfWork uow)
    {
        _companyRepo = companyRepo;
        _termsRepo = termsRepo;
        _paymentMethodRepo = paymentMethodRepo;
        _taxCodeRepo = taxCodeRepo;
        _classRepo = classRepo;
        _locationRepo = locationRepo;
        _uow = uow;
    }

    // === Company Info ===

    [HttpGet("info")]
    public async Task<IActionResult> GetCompanyInfo()
    {
        var info = await _companyRepo.Query().FirstOrDefaultAsync();
        if (info == null) return NotFound();
        return Ok(info);
    }

    [HttpPut("info")]
    public async Task<IActionResult> UpdateCompanyInfo([FromBody] CompanyInfo info)
    {
        var existing = await _companyRepo.Query().FirstOrDefaultAsync();
        if (existing == null) return NotFound();

        existing.Name = info.Name;
        existing.LegalName = info.LegalName;
        existing.Address = info.Address;
        existing.City = info.City;
        existing.State = info.State;
        existing.Zip = info.Zip;
        existing.Phone = info.Phone;
        existing.Email = info.Email;
        existing.EIN = info.EIN;
        existing.FiscalYearStartMonth = info.FiscalYearStartMonth;

        await _companyRepo.UpdateAsync(existing);
        await _uow.SaveChangesAsync();
        return Ok(existing);
    }

    // === Terms ===

    [HttpGet("terms")]
    public async Task<IActionResult> GetTerms()
    {
        var terms = await _termsRepo.GetAllAsync();
        return Ok(terms);
    }

    [HttpPost("terms")]
    public async Task<IActionResult> CreateTerms([FromBody] Terms terms)
    {
        terms.IsActive = true;
        var created = await _termsRepo.AddAsync(terms);
        await _uow.SaveChangesAsync();
        return Ok(created);
    }

    // === Payment Methods ===

    [HttpGet("payment-methods")]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var methods = await _paymentMethodRepo.GetAllAsync();
        return Ok(methods);
    }

    [HttpPost("payment-methods")]
    public async Task<IActionResult> CreatePaymentMethod([FromBody] PaymentMethod method)
    {
        method.IsActive = true;
        var created = await _paymentMethodRepo.AddAsync(method);
        await _uow.SaveChangesAsync();
        return Ok(created);
    }

    // === Tax Codes ===

    [HttpGet("tax-codes")]
    public async Task<IActionResult> GetTaxCodes()
    {
        var codes = await _taxCodeRepo.GetAllAsync();
        return Ok(codes);
    }

    [HttpPost("tax-codes")]
    public async Task<IActionResult> CreateTaxCode([FromBody] TaxCode code)
    {
        code.IsActive = true;
        var created = await _taxCodeRepo.AddAsync(code);
        await _uow.SaveChangesAsync();
        return Ok(created);
    }

    // === Classes ===

    [HttpGet("classes")]
    public async Task<IActionResult> GetClasses()
    {
        var classes = await _classRepo.Query()
            .Include(c => c.SubClasses)
            .Where(c => c.ParentId == null)
            .ToListAsync();
        return Ok(classes);
    }

    [HttpPost("classes")]
    public async Task<IActionResult> CreateClass([FromBody] Class cls)
    {
        cls.IsActive = true;
        var created = await _classRepo.AddAsync(cls);
        await _uow.SaveChangesAsync();
        return Ok(created);
    }

    // === Locations ===

    [HttpGet("locations")]
    public async Task<IActionResult> GetLocations()
    {
        var locations = await _locationRepo.Query()
            .Include(l => l.SubLocations)
            .Where(l => l.ParentId == null)
            .ToListAsync();
        return Ok(locations);
    }

    [HttpPost("locations")]
    public async Task<IActionResult> CreateLocation([FromBody] Location location)
    {
        location.IsActive = true;
        var created = await _locationRepo.AddAsync(location);
        await _uow.SaveChangesAsync();
        return Ok(created);
    }
}

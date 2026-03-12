// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Entities.Vendors;

namespace QBD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly IRepository<Vendor> _repo;
    private readonly IUnitOfWork _uow;

    public VendorsController(IRepository<Vendor> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize,
            filter: activeOnly == true ? v => v.IsActive : null,
            orderBy: v => v.VendorName);
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vendor = await _repo.Query()
            .Include(v => v.Bills)
            .Include(v => v.Terms)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (vendor == null) return NotFound();
        return Ok(vendor);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Vendor vendor)
    {
        vendor.IsActive = true;
        var created = await _repo.AddAsync(vendor);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Vendor vendor)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.VendorName = vendor.VendorName;
        existing.Company = vendor.Company;
        existing.Address = vendor.Address;
        existing.Phone = vendor.Phone;
        existing.Email = vendor.Email;
        existing.TermsId = vendor.TermsId;
        existing.CreditLimit = vendor.CreditLimit;
        existing.TaxId = vendor.TaxId;
        existing.Is1099 = vendor.Is1099;
        existing.IsActive = vendor.IsActive;

        await _repo.UpdateAsync(existing);
        await _uow.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vendor = await _repo.GetByIdAsync(id);
        if (vendor == null) return NotFound();

        await _repo.DeleteAsync(vendor);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
